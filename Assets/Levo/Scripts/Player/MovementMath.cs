using UnityEngine;

namespace Levo.Player
{
    /// <summary>
    /// The pure math behind Source-style movement. No Unity components, no input, no physics,
    /// so it can be unit-tested directly. PlayerMovement calls these every frame.
    /// All methods only change the horizontal (x, z) part of the velocity.
    /// </summary>
    public static class MovementMath
    {
        /// <summary>Ground friction: slows the player, never reverses them.</summary>
        public static Vector3 ApplyFriction(Vector3 velocity, float friction, float stopSpeed, float dt)
        {
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            if (speed < 0.01f)
            {
                velocity.x = 0f;
                velocity.z = 0f;
                return velocity;
            }

            float control = Mathf.Max(speed, stopSpeed);
            float drop = control * friction * dt;
            float scale = Mathf.Max(speed - drop, 0f) / speed;
            velocity.x *= scale;
            velocity.z *= scale;
            return velocity;
        }

        /// <summary>
        /// Source-style acceleration: only adds speed in the wish direction, and never past wishSpeed.
        /// </summary>
        public static Vector3 Accelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel, float dt)
        {
            float currentSpeed = velocity.x * wishDir.x + velocity.z * wishDir.z;
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return velocity;

            float accelSpeed = Mathf.Min(accel * wishSpeed * dt, addSpeed);
            velocity.x += wishDir.x * accelSpeed;
            velocity.z += wishDir.z * accelSpeed;
            return velocity;
        }

        public static bool IsFinite(Vector3 v) =>
            !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
              float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
