using NUnit.Framework;
using UnityEngine;
using Levo.Save;

namespace Levo.Tests
{
    public class StateJsonTests
    {
        // ---------- normal ----------
        [Test]
        public void Normal_Valid_player_state_parses()
        {
            string json = JsonUtility.ToJson(new PlayerState { position = new Vector3(1f, 2f, 3f), yaw = 45f });
            Assert.IsTrue(StateJson.TryParse(json, out PlayerState s));
            Assert.AreEqual(new Vector3(1f, 2f, 3f), s.position);
            Assert.AreEqual(45f, s.yaw, 0.0001f);
        }

        // ---------- boundary ----------
        [Test]
        public void Boundary_Position_exactly_at_the_limit_is_allowed_but_just_past_it_is_not()
        {
            Assert.IsTrue(StateJson.IsReasonablePosition(new Vector3(10000f, -10000f, 0f)));
            Assert.IsFalse(StateJson.IsReasonablePosition(new Vector3(10000.5f, 0f, 0f)));
            Assert.IsFalse(StateJson.IsReasonablePosition(new Vector3(0f, -10001f, 0f)));
        }

        [Test]
        public void Boundary_Identity_rotation_is_valid()
        {
            Assert.IsTrue(StateJson.IsValidRotation(Quaternion.identity));
            Assert.IsTrue(StateJson.IsValidRotation(Quaternion.Euler(10f, 200f, 30f)));
        }

        // ---------- failure ----------
        [Test]
        public void Failure_Garbage_and_empty_text_do_not_parse_and_do_not_throw()
        {
            foreach (string bad in new string[] { null, "", "not json", "{{{" })
                Assert.IsFalse(StateJson.TryParse(bad, out PlayerState _), bad);
        }

        [Test]
        public void Failure_NaN_and_infinity_are_not_reasonable()
        {
            Assert.IsFalse(StateJson.IsReasonablePosition(new Vector3(float.NaN, 0f, 0f)));
            Assert.IsFalse(StateJson.IsReasonablePosition(new Vector3(0f, float.PositiveInfinity, 0f)));
            Assert.IsFalse(StateJson.IsFinite(float.NegativeInfinity));
        }

        [Test]
        public void Failure_All_zero_or_NaN_rotation_is_invalid()
        {
            Assert.IsFalse(StateJson.IsValidRotation(new Quaternion(0f, 0f, 0f, 0f)));
            Assert.IsFalse(StateJson.IsValidRotation(new Quaternion(float.NaN, 0f, 0f, 1f)));
            Assert.IsFalse(StateJson.IsValidRotation(new Quaternion(5f, 5f, 5f, 5f)));
        }
    }
}
