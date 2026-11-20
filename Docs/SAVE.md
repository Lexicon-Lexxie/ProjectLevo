# Levo save and restore (Stage 4)

Press **F5** to save and **F9** to load while playing. The result is shown at the top-left of the screen for a few seconds.

## What is saved
| Object (Save Id) | Component | State |
|---|---|---|
| `player` | `SaveablePlayer` | position and facing (yaw) |
| `prop_01`, ... | `SaveableProp` | position and rotation of each physics prop |
| `door_01`, ... | `SaveableDoor` | open or closed |
| (the scene) | `SaveSystem` | scene name, file version, timestamp, checksum |

Pressure plates are not saved: they work out their state from the props resting on them, so loading puts the props back and the plate follows.
Not saved (known limits): camera pitch, anything the player is holding (it is dropped on load), velocities.

## Where the file is
`Application.persistentDataPath/levo_save.json`. On Windows: `C:\Users\<you>\AppData\LocalLow\<CompanyName>\<ProjectName>\levo_save.json`.
In Unity: the path is printed in the Console when you save, or paste `Application.persistentDataPath` into any script's log.

## Design (modular)
```
Core:   ISaveable (interface)          IPlayerInput (+ QuickSave/QuickLoad)
Save:   SaveSystem  -> finds every ISaveable in the scene, saves/loads them all
        SaveSerializer (file format, validation, checksum)   SaveFileStore (file I/O)
        SaveStates / StateJson (state shapes and range checks)
        SaveablePlayer, SaveableProp, SaveableDoor (small adapters), SaveHotkeys (F5 / F9)
```
- To make something new saveable, add one small component that implements `ISaveable`. `SaveSystem` does not change.
- `SaveableDoor` is an adapter, so `SlidingDoor` stays free of save code.
- Only `PlayerInputReader` reads F5/F9, via `IPlayerInput`.
- Save depends on Core, Player, Interaction and Puzzle. None of those depend on Save.

## Invalid-file handling
Loading is **all-or-nothing**. The whole file and every entry are checked first, and only then is anything changed.
The file is refused, with a message and no change to the game, when it is:
empty, not JSON, truncated, edited by hand (checksum mismatch), from another version, from another scene, has duplicate ids, or contains a value that is not allowed (NaN, infinity, more than 10000 m from the origin, a rotation that is not a rotation).
Entries for objects that no longer exist are ignored. Writing is atomic (temporary file, then replace), so a crash while saving cannot damage an existing save.

## Changes to existing scripts (all additive)
| File | Change |
|---|---|
| `Core/IPlayerInput.cs` | + `QuickSavePressed`, `QuickLoadPressed` |
| `Player/PlayerInputReader.cs` | + F5 / F9 actions |
| `Player/PlayerMovement.cs` | + `ResetVelocity()` |
| `Interaction/PhysicsGrabber.cs` | + `ForceRelease()` |
| `Puzzle/SlidingDoor.cs` | + `SnapTo(bool)` |
| `Tests/PlayMode/FakePlayerInput.cs` | + the two new properties |

## Scene setup
1. Create an empty object `SaveSystem` and add the `SaveSystem` component.
2. On **Player**: add `SaveablePlayer` and `SaveHotkeys`.
3. On `GrabCube`: add `SaveableProp` (Save Id `prop_01`). Every extra prop needs its own id.
4. On `Door`: add `SaveableDoor` (Save Id `door_01`).

## Tests
- EditMode: `SaveSerializerTests` (12), `SaveFileStoreTests` (7), `StateJsonTests` (6).
- PlayMode: `SaveSystemTests` (9), including a simulated close-and-reopen and several bad files.
Run: Window > General > Test Runner > Run All.

## Manual evidence (do these once and take screenshots)
1. **Close and reopen:** Play, move the cube onto the plate (door opens), walk to a corner, press **F5**. Stop Play. Press Play again, press **F9**. You should be back in the corner, the cube on the plate, the door open. Screenshot as `Docs/stage4_restore.png`.
2. **The file:** open `levo_save.json` in a text editor. Screenshot as `Docs/stage4_save_file.png`.
3. **Invalid file:** close the game, change one number in the file (or delete half of it), press Play and **F9**. Expect "Load failed: ... Nothing was changed." and no jump. Screenshot as `Docs/stage4_invalid_file.png`.
4. **No file:** delete the file, press **F9**: "No save file yet."
