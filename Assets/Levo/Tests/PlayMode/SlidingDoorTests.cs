using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Levo.Puzzle;

namespace Levo.Tests
{
    /// <summary>Uses the door's default settings: opens 3 m upward at 2 m/s (1.5 s to fully open).</summary>
    public class SlidingDoorTests : PlayModeTestBase
    {
        SlidingDoor MakeDoor() => Create("Door").AddComponent<SlidingDoor>();

        // ---------- normal ----------
        [UnityTest]
        public IEnumerator Normal_Door_opens_fully_when_told_to()
        {
            var door = MakeDoor();
            door.SetOpen(true);
            yield return new WaitForSeconds(1.7f);
            Assert.AreEqual(3f, door.transform.localPosition.y, 0.01f);
            Assert.IsTrue(door.IsOpen);
        }

        [UnityTest]
        public IEnumerator Normal_Door_closes_again()
        {
            var door = MakeDoor();
            door.SetOpen(true);
            yield return new WaitForSeconds(1.7f);
            door.SetOpen(false);
            yield return new WaitForSeconds(1.7f);
            Assert.AreEqual(0f, door.transform.localPosition.y, 0.01f);
            Assert.IsFalse(door.IsOpen);
        }

        // ---------- boundary ----------
        [UnityTest]
        public IEnumerator Boundary_Closed_door_told_to_close_stays_put()
        {
            var door = MakeDoor();
            door.SetOpen(false);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0f, door.transform.localPosition.y, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Boundary_Door_is_partway_open_after_a_short_time()
        {
            var door = MakeDoor();
            door.SetOpen(true);
            yield return new WaitForSeconds(0.5f);
            float y = door.transform.localPosition.y;
            Assert.Greater(y, 0.2f);
            Assert.Less(y, 2.5f);
        }

        // ---------- failure ----------
        [UnityTest]
        public IEnumerator Failure_Reversing_midway_closes_without_overshooting()
        {
            var door = MakeDoor();
            door.SetOpen(true);
            yield return new WaitForSeconds(0.5f);
            door.SetOpen(false);
            yield return new WaitForSeconds(1.0f);
            float y = door.transform.localPosition.y;
            Assert.GreaterOrEqual(y, -0.0001f);   // never below its closed position
            Assert.AreEqual(0f, y, 0.01f);
        }
    }
}
