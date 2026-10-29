# Wand Spells: meeting scene

Open `Assets/Scenes/WandSpellsMeetingDemo.unity` in Unity 6000.6.4f1 and press Play. This is an isolated visual slice for the proposed Quest 3 wand game. It does not change the robot or surgery scenes.

The rendered Game view is saved as `WandSpellsMeetingPreview.png`. A Quest Android build is saved as `Builds/WandSpellsMeetingDemo.apk`; it compiled successfully, but no headset play test has been performed.

## What you can demonstrate today

- The first-person XR scene, wand, magical chamber, three altars, ambient particles, and three spell effects.
- Press `Q` or `E` to select an altar, then `1` for fire, `2` for frost, or `3` for storm. Press `R` to recenter an incoming IMU orientation.
- An external Python program can send a model result as UDP JSON to port `5055`, for example `{"spell":"fire","conf":0.93}`. The scene sends that result through the same `CastSpell` method as the keyboard demo.

For a loopback check while Unity is in Play mode, run `python wand_demo_sender.py fire` from the project folder. For a headset build, run Python on the computer and use the headset's Wi-Fi IP as `--host` after both are on the same network.

The readout says **KEYBOARD = MOCK IMU / ML** until a real sensor and trained model are connected. This scene alone does not satisfy the professor's IMU or ML requirement.

## Decision for the meeting

Agree on the exact wand hardware and wireless link before promising it. The proposed CC2530/CC2531 path needs a proven transmitter and receiver firmware setup; a stock CC2531 dongle may be a packet sniffer rather than a serial receiver. Check the actual kit together.

Aim for three distinguishable gestures plus idle first: e.g. forward thrust = fire, horizontal sweep = frost, circle = storm. Record labelled six-axis IMU windows from several people, train a simple classifier in Python, and report accuracy on a person excluded from training. Compare the game with IMU predictions enabled and disabled.

The final MR version still needs Quest passthrough, actual wand/hand placement, live IMU data, model training, wireless validation, and on-device performance testing. The current room is a visual direction; the spell targets and effects can be reused when passthrough is added.

## Visual assets

The basalt floor texture was generated for this project with the built-in image generation tool. The wall diffuse and normal textures are Poly Haven's **Medieval Blocks 03**, used under its CC0 license: https://polyhaven.com/a/medieval_blocks_03 .
