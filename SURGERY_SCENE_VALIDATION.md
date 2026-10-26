# Surgery scene validation (27 Oct 2026)

The updated `QuestSurgeryGame.unity` scene was generated and rendered with Unity 6000.6.4f1 in a separate validation copy of the project. The previews are `RobotPlacementPreview.png`, `SurgicalForcepsPreview.png` and `AnatomicalHeartPreview.png`.

An editor validation opened the saved scene and checked that the textured patient, lower-body drape and upper-body surgical drape meshes exist; both anatomical hearts have more than 20 visible mesh parts with size-matched colliders and rigidbodies; the five-piece forceps visual is more than 10 cm across; the disposal tray, donor tray and heart socket exist; and the game controller references the correct hand controller, hearts and targets. It passed. A rendered scene preview confirms the upper-body drape covers the chest outside the surgical opening.

The robot base to native heart, donor heart and disposal tray center distances were 0.375 m, 0.410 m and 0.398 m respectively. These are approximate workspace checks, not a demonstrated joint trajectory or collision-free reach.

Unity rebuilt `Builds/QuestSurgeryGame.mac.apk` successfully for Android ARM64 after the upper-body drape, smaller heart, skin and visible forceps changes. The APK package identifier is `com.ceg5205.questsurgerygame`.

The surgery interaction still needs live hand input in Meta XR Simulator or on a Quest 3. In particular, verify the forceps can reach, grip and release both hearts at each target from the user's position. No physical headset was available for this validation.
