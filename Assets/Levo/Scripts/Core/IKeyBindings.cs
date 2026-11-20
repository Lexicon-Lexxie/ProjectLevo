namespace Levo.Core
{
    /// <summary>Names of the actions whose keys the player may change.</summary>
    public static class BindingNames
    {
        public const string Jump = "Jump";
        public const string Grab = "Grab";
        public const string QuickSave = "QuickSave";
        public const string QuickLoad = "QuickLoad";

        public static readonly string[] All = { Jump, Grab, QuickSave, QuickLoad };
    }

    /// <summary>
    /// Lets other code read and change keys without touching a keyboard or the Input System.
    /// PlayerInputReader is the only real implementation; tests use a fake.
    /// Paths look like "&lt;Keyboard&gt;/space".
    /// </summary>
    public interface IKeyBindings
    {
        string GetBinding(string action);          // current path, or null for an unknown action
        string GetDefaultBinding(string action);
        bool TrySetBinding(string action, string path);
    }
}
