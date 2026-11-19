using UnityEngine;

namespace Levo.Player
{
    /// <summary>
    /// Source-style movement on a CharacterController: ground friction, ground
    /// acceleration, and capped-wish-speed air acceleration (allows air strafing).
    /// Tune the numbers in the Inspector.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Ground")]
        [SerializeField] float maxGroundSpeed = 6f;   // m/s
        [SerializeField] float groundAccel = 10f;
        [SerializeField] float friction = 6f;
        [SerializeField] float stopSpeed = 1.5f;      // friction uses at least this speed so you stop quickly

        [Header("Air")]
        [SerializeField] float airAccel = 12f;
        [SerializeField] float airWishCap = 0.6f;     // small cap = Source-like air strafing

        [Header("Jump / gravity")]
        [SerializeField] float jumpHeight = 1.1f;     // meters
        [SerializeField] float gravity = 20f;
        [SerializeField] bool autoHop = true;         // hold Space to keep jumping

        CharacterController controller;
        PlayerInputReader input;
        Vector3 velocity;

        public Vector3 Velocity => velocity;
        public bool IsGrounded { get; private set; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            IsGrounded = controller.isGrounded;

            Vector2 move = Vector2.ClampMagnitude(input.Move, 1f);
            Vector3 wish = transform.right * move.x + transform.forward * move.y;
            Vector3 wishDir = wish.sqrMagnitude > 0.0001f ? wish.normalized : Vector3.zero;
            float wishSpeed = wish.magnitude * maxGroundSpeed;

            if (IsGrounded)
            {
                if (velocity.y < 0f) velocity.y = -2f;   // keep stuck to the floor
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

            controller.Move(velocity * dt);

            // Hitting a ceiling stops upward motion; walls slide us, so use the real horizontal velocity.
            if ((controller.collisionFlags & CollisionFlags.Above) != 0 && velocity.y > 0f) velocity.y = 0f;
            velocity.x = controller.velocity.x;
            velocity.z = controller.velocity.z;
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
    }
}
