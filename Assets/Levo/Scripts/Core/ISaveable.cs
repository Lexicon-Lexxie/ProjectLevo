namespace Levo.Core
{
    /// <summary>
    /// Anything that has state worth saving. The SaveSystem finds every ISaveable in the scene,
    /// so adding a new saveable thing needs no change to the SaveSystem.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Unique, stable name for this object (e.g. "player", "prop_cube_1").</summary>
        string SaveId { get; }

        /// <summary>Returns this object's state as JSON text.</summary>
        string CaptureState();

        /// <summary>True if the JSON is something this object can safely restore. Must not change anything.</summary>
        bool IsValidState(string json);

        /// <summary>Applies state that IsValidState already accepted.</summary>
        void RestoreState(string json);
    }
}
