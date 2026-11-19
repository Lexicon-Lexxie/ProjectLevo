using System;
using UnityEngine;
using UnityEngine.Events;

namespace Levo.Puzzle
{
    /// <summary>
    /// Base class for anything that can turn on/off (plate, button, sensor).
    /// Targets (doors, lifts) listen to the event, so triggers and targets never
    /// reference each other directly. Wire them in the Inspector.
    /// </summary>
    public abstract class PuzzleTrigger : MonoBehaviour
    {
        [Tooltip("Unique name, used later by the Save system.")]
        [SerializeField] string puzzleId = "trigger_01";
        [SerializeField] UnityEvent<bool> onStateChanged;

        public string PuzzleId => puzzleId;
        public bool IsActive { get; private set; }

        /// <summary>Code-side event (tests and the Save system use this).</summary>
        public event Action<bool> StateChanged;

        protected void SetState(bool value)
        {
            if (value == IsActive) return;
            IsActive = value;
            StateChanged?.Invoke(value);
            onStateChanged?.Invoke(value);
        }
    }
}
