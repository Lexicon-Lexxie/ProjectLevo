using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Levo.Puzzle;

namespace Levo.Tests
{
    /// <summary>The whole puzzle chain: prop on plate -> plate event -> door opens.</summary>
    public class PuzzleIntegrationTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator Normal_Prop_on_plate_opens_the_door_and_removing_it_closes_the_door()
        {
            var plateGo = Create("Plate");
            var box = plateGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2f, 2f);
            var plate = plateGo.AddComponent<PressurePlate>();

            var door = Create("Door").AddComponent<SlidingDoor>();
            plate.StateChanged += door.SetOpen;   // same link the Inspector event makes

            var prop = CreateCube("Prop", Vector3.zero, Vector3.one * 0.5f);
            var body = prop.AddComponent<Rigidbody>();
            body.mass = 2f;
            body.useGravity = false;

            yield return new WaitForSeconds(1.9f);
            Assert.AreEqual(3f, door.transform.localPosition.y, 0.05f, "door should be open");

            prop.transform.position = new Vector3(10f, 0f, 0f);
            yield return new WaitForSeconds(1.9f);
            Assert.AreEqual(0f, door.transform.localPosition.y, 0.05f, "door should be closed again");
        }
    }
}
