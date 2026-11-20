using UnityEngine;
using Levo.Core;
using Levo.Puzzle;

namespace Levo.Save
{
    /// <summary>
    /// Adapter that saves a SlidingDoor's open/closed state. The door itself knows nothing about saving.
    /// Pressure plates are not saved: they work out their state from the props resting on them.
    /// </summary>
    [RequireComponent(typeof(SlidingDoor))]
    public class SaveableDoor : MonoBehaviour, ISaveable
    {
        [SerializeField] string saveId = "door_01";
        public string SaveId => saveId;

        public string CaptureState() => JsonUtility.ToJson(new DoorState { open = GetComponent<SlidingDoor>().IsOpen });

        public bool IsValidState(string json) => StateJson.TryParse(json, out DoorState _);

        public void RestoreState(string json)
        {
            if (!StateJson.TryParse(json, out DoorState s)) return;
            GetComponent<SlidingDoor>().SnapTo(s.open);
        }
    }
}
