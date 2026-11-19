# Levo

A first-person, physics-puzzle exploration game in Unity (C#), inspired by Half-Life style play.
Solo project by Lexxie Ayala for CS110 Final Road.

## Open and run
1. Unity Hub -> Add project from disk -> select this folder.
2. Open with the Unity 6 version shown in `ProjectSettings/ProjectVersion.txt`
   (High Definition 3D template). First open is slow while HDRP shaders compile.
3. Open `Assets/Levo/Scenes/TestRoom.unity` and press Play.
4. Controls: WASD move, mouse look, Space jump (hold to bunny-hop), E pick up / drop,
   Left click throw, Esc free the mouse.

## Folder map
| Path | Contents |
|---|---|
| `Assets/Levo/Scripts/Player` | input reader, Source-style movement, mouse look |
| `Assets/Levo/Scripts/Interaction` | grab / carry / throw physics props |
| `Assets/Levo/Scripts/Puzzle` | triggers (pressure plate) and targets (sliding door) |
| `Assets/Levo/Scenes` | test room scene |
| `Docs/` | whiteboard, evidence screenshots |

## Status
- Stage 1 (Core): Done
- Stages 2-6 (Modular, Tests, Save, Settings, Accessibility): not started.

## Credits
Unity High Definition 3D template content is by Unity Technologies. No Valve assets are used.
