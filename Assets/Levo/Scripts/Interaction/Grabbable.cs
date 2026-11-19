using UnityEngine;

namespace Levo.Interaction
{
    /// <summary>Marker: add to any Rigidbody prop the player is allowed to pick up.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        [SerializeField] float maxCarryMass = 10f;
        public float MaxCarryMass => maxCarryMass;
    }
}
