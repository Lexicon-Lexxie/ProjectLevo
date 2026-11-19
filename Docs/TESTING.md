# Levo automated tests (Stage 3)

Tests use the Unity Test Framework and live in `Assets/Levo/Tests/`.

| Folder | Assembly | What it tests | Runs |
|---|---|---|---|
| `EditMode/` | `Levo.Tests.EditMode` | `MovementMath` (pure math) | instantly, no Play mode |
| `PlayMode/` | `Levo.Tests.PlayMode` | `PressurePlate`, `SlidingDoor`, plate-to-door chain, `PlayerMovement`, `PhysicsGrabber` | in a temporary scene, ~1 minute |

## How to run
Window > General > **Test Runner**. Pick **EditMode** or **PlayMode**, then **Run All**. All tests should be green.

## Test types (every file uses the same naming)
- `Normal_...` expected everyday behavior
- `Boundary_...` exact limits (mass exactly 10, speed exactly at max, zero input)
- `Failure_...` bad or missing things (destroyed prop, missing input component, huge frame time)

## Why these tests are possible
- `MovementMath` is a plain static class, so it needs no scene.
- `IPlayerInput` lets `FakePlayerInput` stand in for a keyboard, so movement and grabbing can be driven by code.
- Puzzle triggers and targets only talk through events, so a test can connect them with one line.

## Defect the suite detects (evidence)
1. Run all tests: they pass (screenshot `Docs/stage3_tests_pass.png`).
2. In `Assets/Levo/Scripts/Player/MovementMath.cs`, in `ApplyFriction`, change
   `float scale = Mathf.Max(speed - drop, 0f) / speed;`
   to
   `float scale = (speed + drop) / speed;` (friction now speeds you up).
3. Run EditMode again: the friction tests fail (screenshot `Docs/stage3_defect_caught.png`).
4. Put the line back and run again: all green.

Optional second defect (plate): in `PressurePlate.cs` change `mass >= requiredMass` to `mass > requiredMass`. The test `Boundary_Mass_exactly_at_the_requirement_activates` fails.

## Known issue found by hand (not tested)
If a prop is stuck under a closing door, the door moves through it instead of stopping. `SlidingDoor` moves by changing its position and does not check for obstacles.
