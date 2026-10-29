using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Meeting vertical slice. Keyboard is a simulated classifier; the same
/// CastSpell entry point accepts predictions from the Python IMU model.
/// </summary>
public sealed class WandSpellDemoController : MonoBehaviour
{
    [Serializable]
    sealed class SensorPacket
    {
        public string spell;
        public float conf;
        public float[] q;
    }

    public Transform wand;
    public Transform wandTip;
    public Transform[] targets;
    public Renderer[] targetCrystals;
    public TextMesh statusText;
    public TextMesh scoreText;
    public int udpPort = 5055;

    readonly Queue<string> incoming = new Queue<string>();
    UdpClient receiver;
    Thread reader;
    volatile bool listening;
    int selected;
    int score;
    float lastSpellTime;
    Quaternion recenter = Quaternion.identity;
    Vector3[] crystalHome;

    void Start()
    {
        crystalHome = new Vector3[targets.Length];
        for (var i = 0; i < targets.Length; ++i) crystalHome[i] = targets[i].localPosition;
        UpdateLabels("SELECT A CRYSTAL  |  Q / E", "KEYBOARD SIMULATES IMU + ML");
        try
        {
            receiver = new UdpClient(udpPort);
            receiver.Client.ReceiveTimeout = 250;
            listening = true;
            reader = new Thread(ReadPackets) { IsBackground = true, Name = "Wand spell UDP" };
            reader.Start();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Wand UDP input unavailable: " + exception.Message);
        }
    }

    void OnDestroy()
    {
        listening = false;
        receiver?.Close();
        if (reader != null && reader.IsAlive) reader.Join(300);
    }

    void ReadPackets()
    {
        var endpoint = new IPEndPoint(IPAddress.Any, 0);
        while (listening)
        {
            try
            {
                var bytes = receiver.Receive(ref endpoint);
                var json = Encoding.UTF8.GetString(bytes);
                lock (incoming)
                {
                    if (incoming.Count < 32) incoming.Enqueue(json);
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { break; }
        }
    }

    void Update()
    {
        for (var i = 0; i < targets.Length; ++i)
        {
            targets[i].Rotate(Vector3.up, Time.deltaTime * (24f + i * 7f), Space.World);
            targets[i].localPosition = crystalHome[i] +
                Vector3.up * (0.035f * Mathf.Sin(Time.time * 1.8f + i * 1.9f));
        }
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.qKey.wasPressedThisFrame) Select(-1);
            if (keyboard.eKey.wasPressedThisFrame) Select(1);
            if (keyboard.digit1Key.wasPressedThisFrame) CastSpell("fire", 1f, "SIMULATED ML");
            if (keyboard.digit2Key.wasPressedThisFrame) CastSpell("frost", 1f, "SIMULATED ML");
            if (keyboard.digit3Key.wasPressedThisFrame) CastSpell("storm", 1f, "SIMULATED ML");
            if (keyboard.rKey.wasPressedThisFrame) recenter = Quaternion.Inverse(wand.localRotation);
        }

        while (true)
        {
            string json;
            lock (incoming)
            {
                if (incoming.Count == 0) break;
                json = incoming.Dequeue();
            }
            try
            {
                var packet = JsonUtility.FromJson<SensorPacket>(json);
                if (packet == null) continue;
                if (packet.q != null && packet.q.Length == 4)
                {
                    var rotation = new Quaternion(packet.q[1], packet.q[2], packet.q[3], packet.q[0]);
                    var lengthSquared = rotation.x * rotation.x + rotation.y * rotation.y +
                        rotation.z * rotation.z + rotation.w * rotation.w;
                    if (lengthSquared > 0.1f) wand.localRotation = recenter * rotation.normalized;
                }
                if (!string.IsNullOrEmpty(packet.spell)) CastSpell(packet.spell, packet.conf, "IMU / UDP");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Invalid wand packet: " + exception.Message);
            }
        }
    }

    void Select(int direction)
    {
        selected = (selected + direction + targets.Length) % targets.Length;
        UpdateLabels("AIMING AT CRYSTAL " + (selected + 1), "1 FIRE   2 FROST   3 STORM");
    }

    public void CastSpell(string spell, float confidence, string source)
    {
        if (Time.time - lastSpellTime < 0.25f || targets == null || targets.Length == 0) return;
        spell = spell.Trim().ToLowerInvariant();
        var color = spell == "fire" || spell == "fireball" ? new Color(1f, 0.22f, 0.04f) :
            spell == "frost" || spell == "ice" ? new Color(0.27f, 0.85f, 1f) :
            spell == "storm" || spell == "lightning" ? new Color(0.65f, 0.4f, 1f) : Color.white;
        if (color == Color.white) return;
        lastSpellTime = Time.time;
        var target = targets[selected];
        StartCoroutine(SpellEffect(spell, color, target.position + Vector3.up * 0.15f));
        StartCoroutine(FlashTarget(selected, color));
        ++score;
        UpdateLabels(spell.ToUpperInvariant() + "  |  CRYSTAL " + (selected + 1),
            source + "  " + Mathf.Clamp01(confidence).ToString("P0"));
        if (scoreText != null) scoreText.text = "SPELLS CAST  " + score.ToString("00");
    }

    IEnumerator FlashTarget(int index, Color color)
    {
        var renderer = targetCrystals[index];
        var material = renderer.material;
        var original = material.color;
        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * 2f);
        yield return new WaitForSeconds(0.45f);
        if (renderer != null)
        {
            material.color = original;
            material.SetColor("_EmissionColor", original * 0.55f);
        }
    }

    IEnumerator SpellEffect(string spell, Color color, Vector3 destination)
    {
        var start = wandTip.position;
        if (spell == "storm" || spell == "lightning")
        {
            var bolt = new GameObject("Storm spell - temporary arc");
            var line = bolt.AddComponent<LineRenderer>();
            line.positionCount = 9;
            line.startWidth = 0.035f;
            line.endWidth = 0.008f;
            line.material = Glow(color);
            for (var i = 0; i < 9; ++i)
            {
                var t = i / 8f;
                line.SetPosition(i, Vector3.Lerp(start, destination, t) +
                    (i == 0 || i == 8 ? Vector3.zero : UnityEngine.Random.insideUnitSphere * 0.08f));
            }
            Destroy(bolt, 0.28f);
            yield break;
        }

        var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = spell + " spell projectile";
        Destroy(projectile.GetComponent<Collider>());
        projectile.transform.localScale = Vector3.one * (spell == "fire" ? 0.13f : 0.10f);
        projectile.GetComponent<Renderer>().material = Glow(color);
        var light = projectile.AddComponent<Light>();
        light.color = color;
        light.range = 1.5f;
        light.intensity = 2f;
        var trail = projectile.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.startWidth = 0.09f;
        trail.endWidth = 0f;
        trail.material = Glow(color);
        var elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            projectile.transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / 0.4f));
            yield return null;
        }
        Destroy(projectile, 0.3f);
    }

    static Material Glow(Color color)
    {
        var material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * 2f);
        return material;
    }

    void UpdateLabels(string headline, string subline)
    {
        if (statusText != null) statusText.text = headline + "\n" + subline;
    }
}
