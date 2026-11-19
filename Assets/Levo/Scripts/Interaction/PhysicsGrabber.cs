using System;
using UnityEngine;
using Levo.Player;

namespace Levo.Interaction
{
    /// <summary>
    /// E = pick up / drop the prop you are looking at. Left click = throw.
    /// Held props are driven by velocity so they still collide with the world.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader), typeof(CharacterController))]
    public class PhysicsGrabber : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] float reach = 3.5f;
        [SerializeField] float holdDistance = 2f;
        [SerializeField] float holdStrength = 15f;
        [SerializeField] float maxHoldSpeed = 12f;
        [SerializeField] float throwSpeed = 10f;

        PlayerInputReader input;
        CharacterController playerCollider;
        Rigidbody held;
        Collider[] heldColliders;
        bool heldHadGravity;
        bool isHolding;

        public bool IsHolding => isHolding;
        public event Action<Rigidbody> Grabbed;
        public event Action<Rigidbody> Released;

        void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            playerCollider = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (isHolding && held == null) ForgetHeld();   // prop was destroyed while held

            if (input.GrabPressed)
            {
                if (isHolding) Release(); else TryGrab();
            }
            else if (isHolding && input.ThrowPressed)
            {
                Throw();
            }
        }

        void FixedUpdate()
        {
            if (!isHolding || held == null) return;

            Vector3 target = cameraTransform.position + cameraTransform.forward * holdDistance;
            Vector3 toTarget = target - held.position;

            if (toTarget.magnitude > reach + 1f) { Release(); return; }   // got stuck behind something

            held.linearVelocity = Vector3.ClampMagnitude(toTarget * holdStrength, maxHoldSpeed);
            held.angularVelocity = Vector3.zero;
        }

        void TryGrab()
        {
            if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                    out RaycastHit hit, reach, ~0, QueryTriggerInteraction.Ignore))
                return;

            Rigidbody body = hit.rigidbody;
            if (body == null || body.isKinematic) return;

            var grabbable = body.GetComponent<Grabbable>();
            if (grabbable == null || body.mass > grabbable.MaxCarryMass) return;

            held = body;
            isHolding = true;
            heldHadGravity = body.useGravity;
            body.useGravity = false;

            heldColliders = body.GetComponentsInChildren<Collider>();
            foreach (var c in heldColliders) Physics.IgnoreCollision(playerCollider, c, true);

            Grabbed?.Invoke(held);
        }

        void Release()
        {
            if (held == null) { ForgetHeld(); return; }
            Rigidbody body = held;
            body.useGravity = heldHadGravity;
            ForgetHeld();
            Released?.Invoke(body);
        }

        void Throw()
        {
            Rigidbody body = held;
            Release();
            if (body != null)
                body.AddForce(cameraTransform.forward * throwSpeed, ForceMode.VelocityChange);
        }

        void ForgetHeld()
        {
            if (heldColliders != null)
                foreach (var c in heldColliders)
                    if (c != null) Physics.IgnoreCollision(playerCollider, c, false);
            heldColliders = null;
            held = null;
            isHolding = false;
        }
    }
}
