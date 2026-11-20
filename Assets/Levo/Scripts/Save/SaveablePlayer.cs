using UnityEngine;
using Levo.Core;
using Levo.Interaction;
using Levo.Player;

namespace Levo.Save
{
    /// <summary>Saves the player's position and facing (yaw). Camera pitch is not saved.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class SaveablePlayer : MonoBehaviour, ISaveable
    {
        [SerializeField] string saveId = "player";
        public string SaveId => saveId;

        public string CaptureState() => JsonUtility.ToJson(new PlayerState
        {
            position = transform.position,
            yaw = transform.eulerAngles.y
        });

        public bool IsValidState(string json) =>
            StateJson.TryParse(json, out PlayerState s) &&
            StateJson.IsReasonablePosition(s.position) &&
            StateJson.IsFinite(s.yaw);

        public void RestoreState(string json)
        {
            if (!StateJson.TryParse(json, out PlayerState s)) return;

            var grabber = GetComponent<PhysicsGrabber>();
            if (grabber != null) grabber.ForceRelease();      // drop whatever is held, or it would be dragged along

            var controller = GetComponent<CharacterController>();
            controller.enabled = false;                       // a CharacterController ignores direct teleports while enabled
            transform.SetPositionAndRotation(s.position, Quaternion.Euler(0f, s.yaw, 0f));
            controller.enabled = true;

            var movement = GetComponent<PlayerMovement>();
            if (movement != null) movement.ResetVelocity();
        }
    }
}
