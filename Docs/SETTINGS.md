# Persistent settings (R04)

Mouse sensitivity, volume and four keys can be changed in a settings panel (**Tab**), are saved to their own file and come back when the game is reopened.

| Setting | Allowed values | Default |
|---|---|---|
| Mouse sensitivity | 0.01 to 1 | 0.10 |
| Volume | 0 to 1 (0% to 100%) | 1 |
| Jump / Grab / QuickSave / QuickLoad keys | one of the allowed keys (a-z except W A S D, 0-9, Space, Left Shift, Left Ctrl, F1-F12), no two actions on the same key | Space / E / F5 / F9 |

W A S D (movement), Esc and Tab are fixed and cannot be assigned, so an action can never be moved onto a key that already means something else.

## File
`levo_settings.json` in Unity's per-user data folder (Edit > Preferences does not show it; in the Console, the line "Settings saved." and `SettingsManager.SettingsPath` give the location; on Windows it is `%userprofile%\AppData\LocalLow\<Company>\<Project>\`). It is separate from `levo_save.json`, so resetting progress never resets settings.

## How it fits the architecture
```
Settings/
  SettingsData        the shape of the file
  SettingsRules       defaults + what is valid (pure logic, unit tested)
  SettingsSerializer  JSON in/out, parsing also validates
  SettingsManager     load, validate, apply, save (reuses Save/SaveFileStore for atomic writes)
  SettingsPanel       on-screen window (Tab); sliders and arrow buttons, never reads a device
Core/IKeyBindings     read/change keys without touching the Input System
Player/PlayerInputReader implements IKeyBindings (still the ONLY class that touches devices)
```
- Settings depends on Core, Player (`PlayerLook.Sensitivity`) and Save (`SaveFileStore`). Nothing else depends on Settings.
- Keys are changed in exactly one place, `PlayerInputReader.TrySetBinding`.
- Nothing is applied unless the **whole** settings object validates first.

## Invalid-file handling
On start the game reads the file. If it is missing, empty, not JSON, truncated, from another version, has a value out of range (or NaN), an unknown or missing action, a key that is reserved, malformed or used twice, then:
1. the defaults are applied, the game keeps running;
2. a message says why ("... Using defaults. The file was not changed.");
3. the bad file is **not overwritten** until you press Save, so you can fix it by hand.

## Changes to existing scripts (additive)
| File | Change |
|---|---|
| `Core/IPlayerInput.cs` | + `SettingsPressed` (Tab) |
| `Player/PlayerInputReader.cs` | implements `IKeyBindings`; + Settings action |
| `Player/PlayerLook.cs` | + `SetPaused(bool)` (frees the mouse while the panel is open) |
| `Tests/PlayMode/FakePlayerInput.cs` | + `SettingsPressed` |

## Scene setup
1. Add `SettingsManager` and `SettingsPanel` to **Player** (the same object as `PlayerInputReader` and `PlayerLook`).
2. Press Play, press **Tab**.

## Tests
- EditMode: `SettingsRulesTests` (28 cases incl. 8 reserved/malformed keys), `SettingsSerializerTests` (11).
- PlayMode: `SettingsManagerTests` (11), including a simulated close-and-reopen and a garbage file.
Run: Window > General > Test Runner > Run All. Expect all earlier tests to still pass.

## Manual evidence
1. **Change and persist:** Play, Tab, drag sensitivity to about 0.50, volume to about 40%, change Jump to Left Shift with the arrows, press **Save**. Stop Play, press Play again: Tab shows the same values and Left Shift jumps. Screenshots `Docs/stage5_panel.png` (before closing) and `Docs/stage5_restored.png` (after reopening).
2. **The file:** open `levo_settings.json`. Screenshot `Docs/stage5_file.png`.
3. **Invalid file:** stop Play, change `"sensitivity"` to `50` (or delete half the file), press Play. Expect the defaults, a warning in the Console and no crash. Screenshot `Docs/stage5_invalid_file.png`.
4. **Missing file:** delete the file, press Play: defaults, "No settings file yet."

## Known limit
Left-clicking a panel button while holding a prop also throws the prop (the grabber does not know the panel is open). Put the prop down first.
