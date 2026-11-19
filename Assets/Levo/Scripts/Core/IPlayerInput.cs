namespace Levo.Core
{
    /// <summary>
    /// Everything the game needs to know about the player's input. Gameplay code depends on this
    /// interface, never on a keyboard or mouse. The real implementation is PlayerInputReader;
    /// tests (Stage 3) can use a fake one.
    /// </summary>
    public interface IPlayerInput
    {
        UnityEngine.Vector2 Move { get; }     // WASD
        UnityEngine.Vector2 Look { get; }     // mouse delta in pixels
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool GrabPressed { get; }             // pick up / drop
        bool PrimaryPressed { get; }          // left mouse: throw, re-capture the cursor
        bool CancelPressed { get; }           // Esc: release the cursor
    }
}
