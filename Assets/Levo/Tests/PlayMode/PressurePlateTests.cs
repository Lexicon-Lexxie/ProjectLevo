using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Levo.Puzzle;

namespace Levo.Tests
{
    public class PressurePlateTests : PlayModeTestBase
    {
        PressurePlate MakePlate()
        {
            var go = Create("Plate");
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2f, 2f);
            return go.AddComponent<PressurePlate>();   // default required mass = 1
        }

        GameObject MakeProp(float mass, Vector3 position)
        {
            var go = CreateCube("Prop", position, Vector3.one * 0.5f);
            var body = go.AddComponent<Rigidbody>();
            body.mass = mass;
            body.useGravity = false;
            return go;
        }

        static IEnumerator Settle() { yield return new WaitForSeconds(0.15f); }

        // ---------- normal ----------
        [UnityTest]
        public IEnumerator Normal_Heavy_prop_activates_the_plate()
        {
            var plate = MakePlate();
            MakeProp(2f, Vector3.zero);
            yield return Settle();
            Assert.IsTrue(plate.IsActive);
        }

        [UnityTest]
        public IEnumerator Normal_Removing_the_prop_deactivates_the_plate()
        {
            var plate = MakePlate();
            var prop = MakeProp(2f, Vector3.zero);
            yield return Settle();
            Assert.IsTrue(plate.IsActive);

            prop.transform.position = new Vector3(10f, 0f, 0f);
            yield return Settle();
            Assert.IsFalse(plate.IsActive);
        }

        [UnityTest]
        public IEnumerator Normal_Plate_fires_its_event_when_state_changes()
        {
            var plate = MakePlate();
            int calls = 0;
            bool lastValue = false;
            plate.StateChanged += v => { calls++; lastValue = v; };

            MakeProp(2f, Vector3.zero);
            yield return Settle();

            Assert.AreEqual(1, calls);
            Assert.IsTrue(lastValue);
        }

        // ---------- boundary ----------
        [UnityTest]
        public IEnumerator Boundary_Mass_exactly_at_the_requirement_activates()
        {
            var plate = MakePlate();
            MakeProp(1f, Vector3.zero);
            yield return Settle();
            Assert.IsTrue(plate.IsActive);
        }

        [UnityTest]
        public IEnumerator Boundary_Mass_just_below_the_requirement_does_not_activate()
        {
            var plate = MakePlate();
            MakeProp(0.9f, Vector3.zero);
            yield return Settle();
            Assert.IsFalse(plate.IsActive);
        }

        [UnityTest]
        public IEnumerator Boundary_Two_light_props_add_up_to_activate()
        {
            var plate = MakePlate();
            MakeProp(0.6f, new Vector3(-0.4f, 0f, 0f));
            MakeProp(0.6f, new Vector3(0.4f, 0f, 0f));
            yield return Settle();
            Assert.IsTrue(plate.IsActive);
        }

        // ---------- failure ----------
        [UnityTest]
        public IEnumerator Failure_Destroying_the_prop_on_the_plate_deactivates_it()
        {
            var plate = MakePlate();
            var prop = MakeProp(2f, Vector3.zero);
            yield return Settle();
            Assert.IsTrue(plate.IsActive);

            Object.Destroy(prop);
            yield return Settle();
            Assert.IsFalse(plate.IsActive);
        }

        [UnityTest]
        public IEnumerator Failure_Prop_without_a_rigidbody_is_ignored()
        {
            var plate = MakePlate();
            CreateCube("StaticBox", Vector3.zero, Vector3.one * 0.5f);   // no Rigidbody
            yield return Settle();
            Assert.IsFalse(plate.IsActive);
        }
    }
}
