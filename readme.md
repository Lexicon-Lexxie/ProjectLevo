# Levo

A first-person, physics-puzzle exploration game in Unity (C#), inspired by Half-Life style play.
Solo project by Lexxie Ayala for CS110 Final Road.

## Open and run
1. Unity Hub -> Add project from disk -> select this folder.
2. Open with the Unity 6 version shown in `ProjectSettings/ProjectVersion.txt`
   (High Definition 3D template). The first open is slow while HDRP shaders compile.
3. Open `Assets/Scenes/TestRoom.unity` and press Play.

## Controls
| Key | Action |
|---|---|
| W A S D | move |
| Mouse | look |
| Space | jump (hold to bunny-hop) |
| E | pick up / drop |
| Left click | throw; also re-captures the mouse |
| Esc | free the mouse |
| F5 / F9 | quick save / quick load |
| Tab | settings panel (sensitivity, volume, keys) |

Jump, Grab, QuickSave and QuickLoad can be changed in the settings panel.

## Folder map
| Path | Contents |
|---|---|
| `Assets/Levo/Scripts/Core` | interfaces: `IPlayerInput`, `IKeyBindings`, `ISaveable` |
| `Assets/Levo/Scripts/Player` | input reader (the only class that reads devices), Source-style movement, mouse look |
| `Assets/Levo/Scripts/Interaction` | grab / carry / throw physics props |
| `Assets/Levo/Scripts/Puzzle` | triggers (pressure plate) and targets (sliding door, light) |
| `Assets/Levo/Scripts/Save` | save and restore progress (F5 / F9) |
| `Assets/Levo/Scripts/Settings` | persistent settings (Tab) |
| `Assets/Levo/Tests` | EditMode and PlayMode tests |
| `Assets/Scenes/TestRoom.unity` | the playable test room |
| `Docs/` | design docs and evidence screenshots (below) |

## Documentation
| File | What it covers |
|---|---|
| `Docs/ARCHITECTURE.md`, `Docs/Levo_class_diagram.png` | modules, responsibilities, dependency rules |
| `Docs/TESTING.md` | how to run the tests, test types, the planted defect |
| `Docs/SAVE.md` | save and restore, invalid-file handling |
| `Docs/SETTINGS.md` | persistent settings, invalid-file handling |

## Run the tests
Window > General > Test Runner > EditMode / PlayMode > Run All. All tests should be green.

## Evidence screenshots (in `Docs/`)
| Property | Files |
|---|---|
| Core | `Stage1_test.png`, `Stage2_test.png`, `Levo_whiteboard.png` |
| Automated tests | `stage3_tests_pass.png`, `stage3_defect_caught.png` |
| Save and restore | `stage4_restore.png`, `stage4_save_file.png`, `stage4_invalid_file.png` |
| Settings | `stage5_panel.png`, `stage5_restored.png`, `stage5_file.png`, `stage5_invalid_file.png` |

## Status
- Stage 1 (Core): Done
- Stage 2 (Modular): Done
- Stage 3 (Tests): Done
- Stage 4 (Save): Done
- Stage 5 (Settings): Done
- Stage 6 (Accessibility):

## Credits
Unity High Definition 3D template content is by Unity Technologies. No Valve assets are used.
