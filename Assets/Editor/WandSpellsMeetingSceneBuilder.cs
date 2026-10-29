using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

/// <summary>Builds an isolated, high-fidelity visual slice for the wand pivot.</summary>
public static class WandSpellsMeetingSceneBuilder
{
    const string ScenePath = "Assets/Scenes/WandSpellsMeetingDemo.unity";
    const string Root = "Assets/WandSpells";

    [MenuItem("Tools/Wand Spells/Build Meeting Scene")]
    public static void BuildScene()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        EnsureFolder(Root + "/Materials");
        EnsureFolder(Root + "/Models");
        var basaltTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/BasaltArcaneFloor.png");
        if (basaltTexture == null) throw new InvalidOperationException("Arcane basalt texture has not imported.");
        var blockTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/MedievalBlocks03_Diffuse.jpg");
        var normalPath = Root + "/Textures/MedievalBlocks03_Normal.jpg";
        var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
        }
        var blockNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        if (blockTexture == null || blockNormal == null)
            throw new InvalidOperationException("CC0 stone wall textures have not imported.");

        var stone = Mat("Basalt stone", new Color(0.24f, 0.28f, 0.32f), null, basaltTexture);
        stone.mainTextureScale = new Vector2(3f, 3f);
        var wallStone = Mat("Chiseled temple stone", new Color(0.53f, 0.59f, 0.66f), null, blockTexture);
        wallStone.mainTextureScale = new Vector2(2f, 2f);
        wallStone.SetTexture("_BumpMap", blockNormal);
        wallStone.EnableKeyword("_NORMALMAP");
        var trim = Mat("Antique gold", new Color(0.65f, 0.48f, 0.23f), null);
        trim.SetFloat("_Metallic", 0.75f); trim.SetFloat("_Glossiness", 0.55f);
        var dark = Mat("Obsidian", new Color(0.028f, 0.042f, 0.063f), null);
        dark.SetFloat("_Metallic", 0.4f); dark.SetFloat("_Glossiness", 0.42f);
        var cyan = Mat("Arcane cyan", new Color(0.08f, 0.72f, 0.95f), new Color(0.04f, 0.52f, 0.95f) * 1.5f);
        var violet = Mat("Arcane violet", new Color(0.62f, 0.28f, 0.92f), new Color(0.40f, 0.14f, 0.75f) * 1.5f);
        var ember = Mat("Arcane ember", new Color(1f, 0.34f, 0.07f), new Color(1f, 0.15f, 0.02f) * 1.6f);
        var parchment = Mat("Warm parchment", new Color(0.91f, 0.80f, 0.55f), null);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.25f, 0.30f, 0.41f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.038f, 0.055f, 0.085f);
        RenderSettings.fogDensity = 0.016f;

        var world = new GameObject("ARCANE SANCTUM - environment").transform;
        Cube("Basalt ground", world, new Vector3(0f, -0.16f, 2.2f), new Vector3(18f, 0.22f, 18f), stone);
        Cube("Rear wall", world, new Vector3(0f, 3.5f, 7.0f), new Vector3(14f, 7f, 0.35f), wallStone);
        Cube("Left wall", world, new Vector3(-6.7f, 3.5f, 2.3f), new Vector3(0.35f, 7f, 10f), wallStone);
        Cube("Right wall", world, new Vector3(6.7f, 3.5f, 2.3f), new Vector3(0.35f, 7f, 10f), wallStone);
        Cube("Upper lintel", world, new Vector3(0f, 5.0f, 4.7f), new Vector3(11.5f, 0.4f, 0.45f), trim);

        Cylinder("Ceremonial dais", world, new Vector3(0f, 0.03f, 1.7f), new Vector3(5.5f, 0.11f, 5.5f), dark);
        Cylinder("Dais gilded rim", world, new Vector3(0f, 0.10f, 1.7f), new Vector3(5.6f, 0.015f, 5.6f), trim);
        Cylinder("Inset ritual floor", world, new Vector3(0f, 0.12f, 1.7f), new Vector3(5.2f, 0.012f, 5.2f), stone);
        for (var i = 0; i < 48; ++i)
        {
            var theta = i * Mathf.PI * 2f / 48f;
            var x = Mathf.Sin(theta) * 2.34f;
            var z = 1.7f + Mathf.Cos(theta) * 2.34f;
            var rune = Cube("Engraved luminous floor glyph " + i, world,
                new Vector3(x, 0.132f, z), new Vector3(i % 4 == 0 ? 0.105f : 0.045f, 0.008f, 0.018f),
                i % 4 == 0 ? trim : cyan);
            rune.rotation = Quaternion.Euler(0f, theta * Mathf.Rad2Deg, 0f);
        }
        for (var i = 0; i < 12; ++i)
        {
            var theta = i * Mathf.PI * 2f / 12f;
            var rune = Cube("Inner circle mark " + i, world,
                new Vector3(Mathf.Sin(theta) * 1.68f, 0.136f, 1.7f + Mathf.Cos(theta) * 1.68f),
                new Vector3(0.028f, 0.008f, 0.23f), cyan);
            rune.rotation = Quaternion.Euler(0f, theta * Mathf.Rad2Deg, 0f);
        }

        foreach (var x in new[] { -4.8f, -3.45f, 3.45f, 4.8f })
        {
            Cylinder("Fluted stone column", world, new Vector3(x, 2.2f, 4.5f),
                new Vector3(0.55f, 2.15f, 0.55f), wallStone);
            Cylinder("Column foot", world, new Vector3(x, 0.18f, 4.5f),
                new Vector3(0.85f, 0.16f, 0.85f), dark);
            Cylinder("Column capital", world, new Vector3(x, 4.35f, 4.5f),
                new Vector3(0.8f, 0.15f, 0.8f), trim);
            for (var band = 1; band <= 3; ++band)
                Cylinder("Column gilded band", world, new Vector3(x, band * 1.06f, 4.5f),
                    new Vector3(0.60f, 0.025f, 0.60f), trim);
        }

        var portalCenter = new Vector3(0f, 2.15f, 6.69f);
        Cylinder("Portal black interior", world, portalCenter, new Vector3(3.0f, 0.035f, 3.0f), dark,
            Quaternion.Euler(90f, 0f, 0f));
        Ring(world, "Portal outer gilding", portalCenter + new Vector3(0f, 0f, -0.04f), 1.75f, 0.10f, 64, trim);
        Ring(world, "Portal arcane core", portalCenter + new Vector3(0f, 0f, -0.11f), 1.53f, 0.045f, 64, cyan);
        Ring(world, "Portal inner tracery", portalCenter + new Vector3(0f, 0f, -0.14f), 1.20f, 0.018f, 64, trim);
        for (var i = 0; i < 16; ++i)
        {
            var theta = i * Mathf.PI * 2f / 16f;
            var glyph = Cube("Portal rune " + i, world,
                portalCenter + new Vector3(Mathf.Cos(theta) * 1.67f, Mathf.Sin(theta) * 1.67f, -0.15f),
                new Vector3(0.035f, 0.13f, 0.025f), cyan);
            glyph.rotation = Quaternion.Euler(0f, 0f, theta * Mathf.Rad2Deg);
        }
        Motes("Portal drifting sparks", world, portalCenter + new Vector3(0f, 0f, -0.32f),
            cyan.color, 2.6f, 1.2f, 22f);

        foreach (var x in new[] { -2.8f, 2.8f })
        {
            var pedestal = Cylinder("Arcane brazier plinth", world, new Vector3(x, 0.65f, 2.65f),
                new Vector3(0.66f, 0.62f, 0.66f), dark);
            Cylinder("Brazier gilded lip", pedestal, new Vector3(0f, 0.68f, 0f),
                new Vector3(0.85f, 0.04f, 0.85f), trim);
            Sphere("Brazier contained flame", world, new Vector3(x, 1.43f, 2.65f),
                new Vector3(0.35f, 0.58f, 0.35f), ember);
            Motes("Brazier flame sparks", world, new Vector3(x, 1.46f, 2.65f),
                ember.color, 0.26f, 0.55f, 11f);
            PointLight("Brazier warm light", new Vector3(x, 1.5f, 2.65f), new Color(1f, 0.35f, 0.13f), 3.2f, 5f);
        }

        var crystalMesh = CrystalMesh();
        var targets = new Transform[3];
        var renderers = new Renderer[3];
        var colors = new[] { ember, cyan, violet };
        var names = new[] { "EMBER", "FROST", "STORM" };
        for (var i = 0; i < 3; ++i)
        {
            var x = (i - 1) * 1.28f;
            var altar = new GameObject("Spell target " + (i + 1) + " - " + names[i]).transform;
            altar.SetParent(world, false);
            altar.position = new Vector3(x, 0f, 3.35f);
            Cylinder("Altar base", altar, new Vector3(0f, 0.37f, 0f), new Vector3(0.72f, 0.36f, 0.72f), dark);
            Cylinder("Altar rim", altar, new Vector3(0f, 0.74f, 0f), new Vector3(0.84f, 0.045f, 0.84f), trim);
            var crystal = new GameObject("Faceted spell crystal");
            crystal.transform.SetParent(altar, false);
            crystal.transform.localPosition = new Vector3(0f, 1.20f, 0f);
            crystal.transform.localScale = Vector3.one * 0.52f;
            crystal.AddComponent<MeshFilter>().sharedMesh = crystalMesh;
            renderers[i] = crystal.AddComponent<MeshRenderer>();
            renderers[i].sharedMaterial = colors[i];
            targets[i] = crystal.transform;
            Text("Altar label", altar, new Vector3(0f, 0.20f, -0.38f), names[i], 0.030f, parchment);
            PointLight("Crystal glow", new Vector3(x, 1.36f, 3.35f), colors[i].color, 1.5f, 2.2f);
        }

        // The XR origin is independent of the old robot/surgery scenes.
        var rig = new GameObject("XR Origin - Wand Spells");
        rig.transform.position = new Vector3(0f, 0f, -3.25f);
        var offset = new GameObject("Camera Offset");
        offset.transform.SetParent(rig.transform, false);
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(offset.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.028f, 0.044f, 0.075f);
        camera.nearClipPlane = 0.04f;
        camera.farClipPlane = 40f;
        camera.fieldOfView = 72f;
        camera.stereoTargetEye = StereoTargetEyeMask.Both;
        cameraObject.AddComponent<AudioListener>();
        var tracked = cameraObject.AddComponent<TrackedPoseDriver>();
        tracked.positionInput = new InputActionProperty(new InputAction("Head position",
            binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3"));
        tracked.rotationInput = new InputActionProperty(new InputAction("Head rotation",
            binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion"));
        tracked.trackingStateInput = new InputActionProperty(new InputAction("Head state",
            binding: "<XRHMD>/trackingState", expectedControlType: "Integer"));
        var xr = rig.AddComponent<XROrigin>();
        xr.Camera = camera;
        xr.CameraFloorOffsetObject = offset;
        xr.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;

        var wand = new GameObject("Gold and obsidian spell wand").transform;
        wand.SetParent(cameraObject.transform, false);
        wand.localPosition = new Vector3(0.23f, -0.25f, 0.47f);
        Cylinder("Leather grip", wand, new Vector3(0f, 0f, 0.10f),
            new Vector3(0.056f, 0.17f, 0.056f), dark, Quaternion.Euler(90f, 0f, 0f));
        Cylinder("Inlaid gold spine", wand, new Vector3(0f, 0f, 0.29f),
            new Vector3(0.032f, 0.17f, 0.032f), trim, Quaternion.Euler(90f, 0f, 0f));
        for (var i = 0; i < 4; ++i)
            Cylinder("Wand gold ferrule", wand, new Vector3(0f, 0f, 0.03f + i * 0.084f),
                new Vector3(0.067f, 0.012f, 0.067f), trim, Quaternion.Euler(90f, 0f, 0f));
        Sphere("Luminous wand focus", wand, new Vector3(0f, 0f, 0.48f),
            new Vector3(0.085f, 0.085f, 0.13f), cyan);
        var tip = new GameObject("Spell emission point").transform;
        tip.SetParent(wand, false);
        tip.localPosition = new Vector3(0f, 0f, 0.57f);
        PointLight("Wand tip glow", wand.TransformPoint(new Vector3(0f, 0f, 0.48f)),
            cyan.color, 1.5f, 1.4f, wand);

        Text("Sanctum title", world, new Vector3(0f, 4.76f, 6.36f),
            "THE ARCANE SANCTUM", 0.065f, parchment);
        Text("Controls", world, new Vector3(0f, 4.35f, 6.30f),
            "Q / E SELECT     1 FIRE     2 FROST     3 STORM", 0.033f, parchment);
        var status = Text("Spell readout", world, new Vector3(-3.6f, 2.64f, 5.94f),
            "SELECT A CRYSTAL\nKEYBOARD = MOCK IMU / ML", 0.031f, cyan);
        var score = Text("Score", world, new Vector3(3.6f, 2.64f, 5.94f),
            "SPELLS CAST  00", 0.031f, parchment);
        var controller = new GameObject("Spell director - keyboard mock and UDP model input")
            .AddComponent<WandSpellDemoController>();
        controller.wand = wand;
        controller.wandTip = tip;
        controller.targets = targets;
        controller.targetCrystals = renderers;
        controller.statusText = status;
        controller.scoreText = score;
        PointLight("Portal cyan bounce", new Vector3(0f, 2.1f, 6.0f),
            new Color(0.14f, 0.56f, 1f), 2.6f, 6f);
        var key = new GameObject("Warm key light").AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(1f, 0.81f, 0.60f);
        key.intensity = 1.1f;
        key.transform.rotation = Quaternion.Euler(48f, -24f, 0f);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
            throw new InvalidOperationException("Could not save wand meeting scene.");
        AssetDatabase.SaveAssets();
        Debug.Log("WAND_MEETING_SCENE_SAVED " + ScenePath);
    }

    [MenuItem("Tools/Wand Spells/Build Quest APK")]
    public static void BuildAndroid()
    {
        QuestViewSetup.ConfigureProject();
        if (!System.IO.File.Exists(ScenePath)) BuildScene();
        var previousId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
        var previousName = PlayerSettings.productName;
        var output = System.IO.Path.GetFullPath("Builds/WandSpellsMeetingDemo.apk");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
        try
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,
                "com.ceg5205.wandspellsdemo");
            PlayerSettings.productName = "Wand Spells MR";
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Wand Quest APK failed: " + report.summary.result);
            Debug.Log("WAND_QUEST_APK_BUILT " + output);
        }
        finally
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, previousId);
            PlayerSettings.productName = previousName;
        }
    }

    static void EnsureFolder(string path)
    {
        var pieces = path.Split('/');
        var current = pieces[0];
        for (var i = 1; i < pieces.Length; ++i)
        {
            var next = current + "/" + pieces[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, pieces[i]);
            current = next;
        }
    }

    static Material Mat(string name, Color color, Color? emission, Texture texture = null)
    {
        var path = Root + "/Materials/" + name.Replace(' ', '_') + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.name = name;
        material.color = color;
        material.mainTexture = texture;
        material.SetFloat("_Glossiness", 0.35f);
        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    static Transform Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        => Shape(PrimitiveType.Cube, name, parent, position, scale, material);
    static Transform Cylinder(string name, Transform parent, Vector3 position, Vector3 scale,
        Material material, Quaternion? rotation = null)
        => Shape(PrimitiveType.Cylinder, name, parent, position, scale, material, rotation);
    static Transform Sphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        => Shape(PrimitiveType.Sphere, name, parent, position, scale, material);

    static Transform Shape(PrimitiveType type, string name, Transform parent, Vector3 position,
        Vector3 scale, Material material, Quaternion? rotation = null)
    {
        var shape = GameObject.CreatePrimitive(type);
        shape.name = name;
        shape.transform.SetParent(parent, false);
        shape.transform.localPosition = position;
        shape.transform.localRotation = rotation ?? Quaternion.identity;
        shape.transform.localScale = scale;
        shape.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(shape.GetComponent<Collider>());
        return shape.transform;
    }

    static TextMesh Text(string name, Transform parent, Vector3 position, string content,
        float size, Material material)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.identity;
        var text = go.AddComponent<TextMesh>();
        text.text = content;
        text.characterSize = size;
        text.fontSize = 64;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = material.color;
        return text;
    }

    static Light PointLight(string name, Vector3 position, Color color, float intensity,
        float range, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = position;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    static void Motes(string name, Transform parent, Vector3 position, Color color,
        float radius, float lifetime, float rate)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        var particles = go.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = true;
        main.startLifetime = lifetime;
        main.startSpeed = 0.25f;
        main.startSize = 0.035f;
        main.startColor = color;
        main.maxParticles = 120;
        var emission = particles.emission;
        emission.rateOverTime = rate;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius;
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 0.3f;
    }

    static void Ring(Transform parent, string name, Vector3 center, float radius,
        float thickness, int segments, Material material)
    {
        var ring = new GameObject(name).transform;
        ring.SetParent(parent, false);
        for (var i = 0; i < segments; ++i)
        {
            var a = i * Mathf.PI * 2f / segments;
            var b = (i + 1) * Mathf.PI * 2f / segments;
            var p = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            var q = center + new Vector3(Mathf.Cos(b) * radius, Mathf.Sin(b) * radius, 0f);
            var segment = Cylinder("Gilded arc", ring, (p + q) * 0.5f,
                new Vector3(thickness, Vector3.Distance(p, q) * 0.53f, thickness), material);
            segment.up = (q - p).normalized;
        }
    }

    static Mesh CrystalMesh()
    {
        const string path = Root + "/Models/FacetedSpellCrystal.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        const int sides = 8;
        var vertices = new Vector3[sides * 3 + 2];
        var triangles = new int[sides * 18];
        vertices[0] = new Vector3(0f, 0.78f, 0f);
        vertices[vertices.Length - 1] = new Vector3(0f, -0.72f, 0f);
        for (var ring = 0; ring < 3; ++ring)
            for (var i = 0; i < sides; ++i)
            {
                var angle = i * Mathf.PI * 2f / sides;
                var radius = ring == 1 ? 0.44f : 0.27f;
                var height = ring == 0 ? 0.39f : ring == 1 ? 0.03f : -0.35f;
                vertices[1 + ring * sides + i] = new Vector3(Mathf.Cos(angle) * radius,
                    height, Mathf.Sin(angle) * radius);
            }
        var cursor = 0;
        for (var i = 0; i < sides; ++i)
        {
            var next = (i + 1) % sides;
            triangles[cursor++] = 0; triangles[cursor++] = 1 + next; triangles[cursor++] = 1 + i;
            for (var ring = 0; ring < 2; ++ring)
            {
                var a = 1 + ring * sides + i; var b = 1 + ring * sides + next;
                var c = a + sides; var d = b + sides;
                triangles[cursor++] = a; triangles[cursor++] = b; triangles[cursor++] = c;
                triangles[cursor++] = b; triangles[cursor++] = d; triangles[cursor++] = c;
            }
            triangles[cursor++] = vertices.Length - 1;
            triangles[cursor++] = 1 + 2 * sides + i;
            triangles[cursor++] = 1 + 2 * sides + next;
        }
        var mesh = new Mesh { name = "Faceted spell crystal" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }
}
