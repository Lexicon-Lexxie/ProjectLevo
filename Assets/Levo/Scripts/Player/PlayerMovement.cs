using UnityEngine;
using Levo.Core;

namespace Levo.Player
{
    /// <summary>
    /// Source-style movement on a CharacterController: ground friction, ground
    /// acceleration, and capped-wish-speed air acceleration (allows air strafing).
    /// Reads input only through IPlayerInput.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Ground")]
        [SerializeField] float maxGroundSpeed = 6f;   // m/s
        [SerializeField] float groundAccel = 10f;
        [SerializeField] float friction = 6f;
        [SerializeField] float stopSpeed = 1.5f;

        [Header("Air")]
        [SerializeField] float airAccel = 12f;
        [SerializeField] float airWishCap = 0.6f;

        [Header("Jump / gravity")]
        [SerializeField] float jumpHeight = 1.1f;     // meters
        [SerializeField] float gravity = 20f;
        [SerializeField] bool autoHop = false;

        CharacterController controller;
        IPlayerInput input;
        Vector3 velocity;

        public Vector3 Velocity => velocity;
        public bool IsGrounded { get; private set; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.minMoveDistance = 0f;
            input = GetComponent<IPlayerInput>();
            if (input == null)
            {
                Debug.LogError("PlayerMovement needs a component that implements IPlayerInput.", this);
                enabled = false;
            }
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            IsGrounded = controller.isGrounded;

            Vector2 move = Vector2.ClampMagnitude(input.Move, 1f);
            Vector3 wish = transform.right * move.x + transform.forward * move.y;
            Vector3 wishDir = wish.sqrMagnitude > 0.0001f ? wish.normalized : Vector3.zero;
            float wishSpeed = wish.magnitude * maxGroundSpeed;

            if (IsGrounded)
            {
                if (velocity.y < 0f) velocity.y = -2f;
                ApplyFriction(dt);
                Accelerate(wishDir, wishSpeed, groundAccel, dt);

                bool wantsJump = autoHop ? input.JumpHeld : input.JumpPressed;
                if (wantsJump) velocity.y = Mathf.Sqrt(2f * gravity * jumpHeight);
            }
            else
            {
                Accelerate(wishDir, Mathf.Min(wishSpeed, airWishCap), airAccel, dt);
                velocity.y -= gravity * dt;
            }

            if (!IsFinite(velocity)) velocity = Vector3.zero;
            controller.Move(velocity * dt);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Vector3 normal = hit.normal;
            if (normal.y > 0.7f) return;
            float into = Vector3.Dot(velocity, normal);
            if (into < 0f) velocity -= normal * into;
        }

        void ApplyFriction(float dt)
        {
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            if (speed < 0.01f) { velocity.x = 0f; velocity.z = 0f; return; }

            float control = Mathf.Max(speed, stopSpeed);
            float drop = control * friction * dt;
            float scale = Mathf.Max(speed - drop, 0f) / speed;
            velocity.x *= scale;
            velocity.z *= scale;
        }

        void Accelerate(Vector3 wishDir, float wishSpeed, float accel, float dt)
        {
            float currentSpeed = velocity.x * wishDir.x + velocity.z * wishDir.z;
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return;

            float accelSpeed = Mathf.Min(accel * wishSpeed * dt, addSpeed);
            velocity.x += wishDir.x * accelSpeed;
            velocity.z += wishDir.z * accelSpeed;
        }

        static bool IsFinite(Vector3 v) =>
            !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
              float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
