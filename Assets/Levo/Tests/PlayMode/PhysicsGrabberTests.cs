using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Levo.Interaction;

namespace Levo.Tests
{
    public class PhysicsGrabberTests : PlayModeTestBase
    {
        FakePlayerInput input;
        PhysicsGrabber grabber;

        void BuildPlayer()
        {
            var player = Create("Player");
            player.SetActive(false);   // so Awake runs only after the camera is assigned
            var controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.9f, 0f);
            input = player.AddComponent<FakePlayerInput>();

            var cam = Create("Camera");
            cam.transform.SetParent(player.transform, false);
            cam.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            grabber = player.AddComponent<PhysicsGrabber>();
            SetField(grabber, "cameraTransform", cam.transform);
            player.SetActive(true);
        }

        Rigidbody MakeProp(float mass, bool grabbable = true)
        {
            var go = CreateCube("Prop", new Vector3(0f, 1.6f, 2f), Vector3.one * 0.5f);   // 2 m in front of the camera
            var body = go.AddComponent<Rigidbody>();
            body.mass = mass;
            body.useGravity = false;
            if (grabbable) go.AddComponent<Grabbable>();
            return body;
        }

        IEnumerator PressGrab()
        {
            input.PressGrab();
            yield return null;
            yield return null;
        }

        // ---------- normal ----------
        [UnityTest]
        public IEnumerator Normal_Light_prop_in_front_can_be_picked_up()
        {
            BuildPlayer();
            MakeProp(2f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsTrue(grabber.IsHolding);
        }

        [UnityTest]
        public IEnumerator Normal_Pressing_grab_again_drops_the_prop()
        {
            BuildPlayer();
            MakeProp(2f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsTrue(grabber.IsHolding);
            yield return PressGrab();
            Assert.IsFalse(grabber.IsHolding);
        }

        [UnityTest]
        public IEnumerator Normal_Throwing_releases_the_prop_and_sends_it_forward()
        {
            BuildPlayer();
            var body = MakeProp(2f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return PressGrab();
            Assert.IsTrue(grabber.IsHolding);

            input.PressPrimary();
            yield return null;
            yield return null;
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(grabber.IsHolding);
            Assert.Greater(body.linearVelocity.z, 1f);
        }

        // ---------- boundary ----------
        [UnityTest]
        public IEnumerator Boundary_Prop_exactly_at_the_mass_limit_can_be_picked_up()
        {
            BuildPlayer();
            MakeProp(10f);   // Grabbable default limit is 10
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsTrue(grabber.IsHolding);
        }

        [UnityTest]
        public IEnumerator Boundary_Prop_just_over_the_mass_limit_cannot_be_picked_up()
        {
            BuildPlayer();
            MakeProp(10.5f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsFalse(grabber.IsHolding);
        }

        // ---------- failure ----------
        [UnityTest]
        public IEnumerator Failure_Prop_without_Grabbable_is_ignored()
        {
            BuildPlayer();
            MakeProp(2f, grabbable: false);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsFalse(grabber.IsHolding);
        }

        [UnityTest]
        public IEnumerator Failure_Looking_at_nothing_grabs_nothing()
        {
            BuildPlayer();   // no prop at all
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsFalse(grabber.IsHolding);
        }

        [UnityTest]
        public IEnumerator Failure_Prop_out_of_reach_cannot_be_picked_up()
        {
            BuildPlayer();
            var body = MakeProp(2f);
            body.transform.position = new Vector3(0f, 1.6f, 10f);   // reach is 3.5 m
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            yield return PressGrab();
            Assert.IsFalse(grabber.IsHolding);
        }
    }
}
