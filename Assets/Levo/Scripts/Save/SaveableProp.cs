using UnityEngine;
using Levo.Core;

namespace Levo.Save
{
    /// <summary>Saves a physics prop's position and rotation. Each prop needs its own unique Save Id.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SaveableProp : MonoBehaviour, ISaveable
    {
        [SerializeField] string saveId = "prop_01";
        public string SaveId => saveId;

        public string CaptureState() => JsonUtility.ToJson(new PropState
        {
            position = transform.position,
            rotation = transform.rotation
        });

        public bool IsValidState(string json) =>
            StateJson.TryParse(json, out PropState s) &&
            StateJson.IsReasonablePosition(s.position) &&
            StateJson.IsValidRotation(s.rotation);

        public void RestoreState(string json)
        {
            if (!StateJson.TryParse(json, out PropState s)) return;

            var body = GetComponent<Rigidbody>();
            transform.SetPositionAndRotation(s.position, s.rotation);
            body.position = s.position;
            body.rotation = s.rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }
    }
}
