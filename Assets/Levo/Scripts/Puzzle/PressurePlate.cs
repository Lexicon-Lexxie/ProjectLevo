using System.Collections.Generic;
using UnityEngine;

namespace Levo.Puzzle
{
    /// <summary>
    /// Active while enough physics mass is resting on it. Put this on a GameObject with a
    /// Box Collider set to "Is Trigger" (one collider per prop keeps the counting correct).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PressurePlate : PuzzleTrigger
    {
        [SerializeField] float requiredMass = 1f;
        [SerializeField] bool playerActivates = false;

        readonly HashSet<Rigidbody> bodies = new HashSet<Rigidbody>();
        int playersOn;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CharacterController>() != null) { playersOn++; }
            else if (other.attachedRigidbody != null) { bodies.Add(other.attachedRigidbody); }
            Evaluate();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<CharacterController>() != null) { playersOn = Mathf.Max(0, playersOn - 1); }
            else if (other.attachedRigidbody != null) { bodies.Remove(other.attachedRigidbody); }
            Evaluate();
        }

        void Update()
        {
            // Handles props that get destroyed while on the plate.
            if (bodies.RemoveWhere(b => b == null) > 0) Evaluate();
        }

        void Evaluate()
        {
            float mass = 0f;
            foreach (var b in bodies) mass += b.mass;
            bool pressed = mass >= requiredMass || (playerActivates && playersOn > 0);
            SetState(pressed);
        }
    }
}
