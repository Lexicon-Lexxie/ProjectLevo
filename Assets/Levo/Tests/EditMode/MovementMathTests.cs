using NUnit.Framework;
using UnityEngine;
using Levo.Player;

namespace Levo.Tests
{
    /// <summary>Unit tests for the pure movement math. Categories: normal, boundary, failure.</summary>
    public class MovementMathTests
    {
        const float Dt = 0.016f;

        // ---------- normal ----------
        [Test]
        public void Normal_Accelerate_from_rest_adds_expected_speed()
        {
            Vector3 v = MovementMath.Accelerate(Vector3.zero, Vector3.forward, 6f, 10f, Dt);
            Assert.AreEqual(0.96f, v.z, 0.001f);   // 10 * 6 * 0.016
            Assert.AreEqual(0f, v.x, 0.0001f);
        }

        [Test]
        public void Normal_Accelerate_reaches_max_speed_and_never_passes_it()
        {
            Vector3 v = Vector3.zero;
            float highest = 0f;
            for (int i = 0; i < 300; i++)
            {
                v = MovementMath.Accelerate(v, Vector3.forward, 6f, 10f, Dt);
                highest = Mathf.Max(highest, v.z);
            }
            Assert.AreEqual(6f, v.z, 0.001f);
            Assert.LessOrEqual(highest, 6.0001f);
        }

        [Test]
        public void Normal_Friction_reduces_speed()
        {
            Vector3 v = MovementMath.ApplyFriction(new Vector3(0f, 0f, 5f), 6f, 1.5f, Dt);
            Assert.AreEqual(4.52f, v.z, 0.001f);   // 5 - (5 * 6 * 0.016)
        }

        [Test]
        public void Normal_Friction_eventually_stops_the_player()
        {
            Vector3 v = new Vector3(0f, 0f, 6f);
            for (int i = 0; i < 600; i++) v = MovementMath.ApplyFriction(v, 6f, 1.5f, Dt);
            Assert.AreEqual(0f, v.z, 0.0001f);
        }

        // ---------- boundary ----------
        [Test]
        public void Boundary_Accelerate_with_no_wish_changes_nothing()
        {
            Vector3 start = new Vector3(1f, 0f, 2f);
            Vector3 v = MovementMath.Accelerate(start, Vector3.zero, 0f, 10f, Dt);
            Assert.AreEqual(start, v);
        }

        [Test]
        public void Boundary_Accelerate_at_max_speed_adds_nothing()
        {
            Vector3 v = MovementMath.Accelerate(new Vector3(0f, 0f, 6f), Vector3.forward, 6f, 10f, Dt);
            Assert.AreEqual(6f, v.z, 0.0001f);
        }

        [Test]
        public void Boundary_Air_cap_does_not_remove_existing_speed()
        {
            // Source-style air control: a small wish cap must never slow a fast player down.
            Vector3 v = MovementMath.Accelerate(new Vector3(0f, 0f, 6f), Vector3.forward, 0.6f, 12f, Dt);
            Assert.AreEqual(6f, v.z, 0.0001f);
        }

        [Test]
        public void Boundary_Friction_uses_stop_speed_for_slow_movement()
        {
            Vector3 v = MovementMath.ApplyFriction(new Vector3(0f, 0f, 1f), 6f, 1.5f, Dt);
            Assert.AreEqual(0.856f, v.z, 0.001f);   // 1 - (1.5 * 6 * 0.016)
        }

        [Test]
        public void Boundary_Friction_at_zero_speed_stays_zero()
        {
            Vector3 v = MovementMath.ApplyFriction(Vector3.zero, 6f, 1.5f, Dt);
            Assert.AreEqual(Vector3.zero, v);
        }

        [Test]
        public void Boundary_Friction_snaps_tiny_speed_to_zero()
        {
            Vector3 v = MovementMath.ApplyFriction(new Vector3(0.005f, 0f, 0f), 6f, 1.5f, Dt);
            Assert.AreEqual(0f, v.x);
        }

        [Test]
        public void Boundary_Friction_with_zero_time_changes_nothing()
        {
            Vector3 v = MovementMath.ApplyFriction(new Vector3(0f, 0f, 5f), 6f, 1.5f, 0f);
            Assert.AreEqual(5f, v.z, 0.0001f);
        }

        // ---------- failure ----------
        [Test]
        public void Failure_Friction_with_huge_time_never_reverses_direction()
        {
            Vector3 v = MovementMath.ApplyFriction(new Vector3(0f, 0f, 5f), 6f, 1.5f, 10f);
            Assert.GreaterOrEqual(v.z, 0f);
            Assert.AreEqual(0f, v.z, 0.0001f);
        }

        [Test]
        public void Failure_Movement_math_keeps_vertical_velocity_untouched()
        {
            Vector3 a = MovementMath.Accelerate(new Vector3(0f, 5f, 0f), Vector3.forward, 6f, 10f, Dt);
            Vector3 f = MovementMath.ApplyFriction(new Vector3(0f, -2f, 3f), 6f, 1.5f, Dt);
            Assert.AreEqual(5f, a.y);
            Assert.AreEqual(-2f, f.y);
        }

        [Test]
        public void Failure_IsFinite_detects_NaN_and_infinity()
        {
            Assert.IsTrue(MovementMath.IsFinite(new Vector3(1f, 2f, 3f)));
            Assert.IsFalse(MovementMath.IsFinite(new Vector3(float.NaN, 0f, 0f)));
            Assert.IsFalse(MovementMath.IsFinite(new Vector3(0f, float.PositiveInfinity, 0f)));
            Assert.IsFalse(MovementMath.IsFinite(new Vector3(0f, 0f, float.NegativeInfinity)));
        }
    }
}
