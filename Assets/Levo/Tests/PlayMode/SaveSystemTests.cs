using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Levo.Puzzle;
using Levo.Save;

namespace Levo.Tests
{
    /// <summary>Saves and loads a small world (player, prop, door) through the real SaveSystem and a real file.</summary>
    public class SaveSystemTests : PlayModeTestBase
    {
        string dir;
        SaveSystem saveSystem;
        GameObject player, propGo;
        Rigidbody prop;
        SlidingDoor door;

        static readonly Vector3 PlayerA = new Vector3(4f, 0f, 5f);
        static readonly Vector3 PropA = new Vector3(6f, 1f, 7f);
        const float YawA = 90f;

        static readonly Vector3 PlayerB = new Vector3(-3f, 0f, -4f);
        static readonly Vector3 PropB = new Vector3(-5f, 1f, -6f);
        const float YawB = 200f;

        [SetUp]
        public void MakeTempFolder() =>
            dir = Path.Combine(Path.GetTempPath(), "LevoSaveTests", System.Guid.NewGuid().ToString("N"));

        [TearDown]
        public void DeleteTempFolder()
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
            catch (System.Exception) { }
        }

        static string SceneName() => SceneManager.GetActiveScene().name;

        void BuildWorld()
        {
            saveSystem = Create("SaveSystem").AddComponent<SaveSystem>();
            saveSystem.UseDirectory(dir);

            player = Create("Player");
            player.AddComponent<CharacterController>();
            player.AddComponent<SaveablePlayer>();

            propGo = CreateCube("Prop", new Vector3(0f, 1f, 0f), Vector3.one * 0.5f);
            prop = propGo.AddComponent<Rigidbody>();
            prop.useGravity = false;
            propGo.AddComponent<SaveableProp>();

            var doorGo = Create("Door");
            door = doorGo.AddComponent<SlidingDoor>();
            doorGo.AddComponent<SaveableDoor>();
        }

        void SetWorld(Vector3 playerPos, float yaw, Vector3 propPos, bool doorOpen)
        {
            player.transform.SetPositionAndRotation(playerPos, Quaternion.Euler(0f, yaw, 0f));
            propGo.transform.position = propPos;
            prop.position = propPos;
            prop.linearVelocity = Vector3.zero;
            door.SnapTo(doorOpen);
        }

        void AssertWorld(Vector3 playerPos, float yaw, Vector3 propPos, bool doorOpen)
        {
            Assert.Less((player.transform.position - playerPos).magnitude, 0.01f, "player position");
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)), 0.5f, "player yaw");
            Assert.Less((propGo.transform.position - propPos).magnitude, 0.01f, "prop position");
            Assert.AreEqual(doorOpen, door.IsOpen, "door open flag");
            Assert.AreEqual(doorOpen ? 3f : 0f, door.transform.localPosition.y, 0.01f, "door position");
        }

        void WriteFile(string text)
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(saveSystem.SavePath, text);
        }

        static SaveEntry Entry(string id, object state) =>
            new SaveEntry { id = id, json = JsonUtility.ToJson(state) };

        // ---------- normal ----------
        [UnityTest]
        public IEnumerator Normal_Save_then_load_restores_player_prop_and_door()
        {
            BuildWorld();
            yield return null;

            SetWorld(PlayerA, YawA, PropA, doorOpen: true);
            Assert.IsTrue(saveSystem.Save());
            Assert.IsTrue(File.Exists(saveSystem.SavePath));

            SetWorld(PlayerB, YawB, PropB, doorOpen: false);
            Assert.AreEqual(LoadResult.Loaded, saveSystem.Load());
            yield return null;

            AssertWorld(PlayerA, YawA, PropA, doorOpen: true);
        }

        [UnityTest]
        public IEnumerator Normal_State_survives_a_simulated_restart()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);
            Assert.IsTrue(saveSystem.Save());

            // "Close the program": destroy every object, including the SaveSystem itself.
            foreach (var go in new[] { saveSystem.gameObject, player, propGo, door.gameObject })
                Object.Destroy(go);
            yield return null;
            yield return null;

            // "Reopen": brand-new objects in their default places, a brand-new SaveSystem reading the file.
            BuildWorld();
            yield return null;
            SetWorld(PlayerB, YawB, PropB, doorOpen: false);

            Assert.AreEqual(LoadResult.Loaded, saveSystem.Load());
            yield return null;
            AssertWorld(PlayerA, YawA, PropA, doorOpen: true);
        }

        // ---------- boundary ----------
        [UnityTest]
        public IEnumerator Boundary_Load_with_no_save_file_reports_it_and_changes_nothing()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerB, YawB, PropB, doorOpen: false);

            Assert.AreEqual(LoadResult.NoSaveFile, saveSystem.Load());
            AssertWorld(PlayerB, YawB, PropB, doorOpen: false);
        }

        [UnityTest]
        public IEnumerator Boundary_Saving_twice_keeps_only_the_latest_save()
        {
            BuildWorld();
            yield return null;

            SetWorld(PlayerA, YawA, PropA, doorOpen: true);
            Assert.IsTrue(saveSystem.Save());
            SetWorld(PlayerB, YawB, PropB, doorOpen: false);
            Assert.IsTrue(saveSystem.Save());
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);

            Assert.AreEqual(LoadResult.Loaded, saveSystem.Load());
            yield return null;
            AssertWorld(PlayerB, YawB, PropB, doorOpen: false);
        }

        [UnityTest]
        public IEnumerator Boundary_Entries_for_objects_that_no_longer_exist_are_ignored()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);

            WriteFile(SaveSerializer.ToJson(SceneName(), new[]
            {
                Entry("player", new PlayerState { position = PlayerB, yaw = YawB }),
                Entry("ghost_object", new DoorState { open = true })
            }));

            Assert.AreEqual(LoadResult.Loaded, saveSystem.Load());
            yield return null;

            Assert.Less((player.transform.position - PlayerB).magnitude, 0.01f);
            Assert.Less((propGo.transform.position - PropA).magnitude, 0.01f, "prop was not in the file, so it must keep its state");
        }

        // ---------- failure ----------
        [UnityTest]
        public IEnumerator Failure_Garbage_file_is_rejected_and_the_world_is_unchanged()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);
            WriteFile("{{{ this is not a save file");

            Assert.AreEqual(LoadResult.Invalid, saveSystem.Load());

            StringAssert.Contains("Nothing was changed", saveSystem.LastMessage);
            AssertWorld(PlayerA, YawA, PropA, doorOpen: true);
        }

        [UnityTest]
        public IEnumerator Failure_Truncated_save_file_is_rejected_and_the_world_is_unchanged()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);
            Assert.IsTrue(saveSystem.Save());

            string text = File.ReadAllText(saveSystem.SavePath);
            File.WriteAllText(saveSystem.SavePath, text.Substring(0, text.Length / 2));
            SetWorld(PlayerB, YawB, PropB, doorOpen: false);

            Assert.AreEqual(LoadResult.Invalid, saveSystem.Load());

            AssertWorld(PlayerB, YawB, PropB, doorOpen: false);
        }

        [UnityTest]
        public IEnumerator Failure_One_bad_entry_means_nothing_is_restored()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);

            // Player state is fine, but the prop is 1e30 meters away: the whole file must be refused.
            WriteFile(SaveSerializer.ToJson(SceneName(), new[]
            {
                Entry("player", new PlayerState { position = PlayerB, yaw = YawB }),
                Entry("prop_01", new PropState { position = new Vector3(1e30f, 0f, 0f) }),
                Entry("door_01", new DoorState { open = false })
            }));

            Assert.AreEqual(LoadResult.Invalid, saveSystem.Load());

            AssertWorld(PlayerA, YawA, PropA, doorOpen: true);   // not even the valid player entry was applied
        }

        [UnityTest]
        public IEnumerator Failure_Save_from_a_different_scene_is_rejected()
        {
            BuildWorld();
            yield return null;
            SetWorld(PlayerA, YawA, PropA, doorOpen: true);

            WriteFile(SaveSerializer.ToJson("SomeOtherScene", new[]
            {
                Entry("player", new PlayerState { position = PlayerB, yaw = YawB })
            }));

            Assert.AreEqual(LoadResult.WrongScene, saveSystem.Load());

            AssertWorld(PlayerA, YawA, PropA, doorOpen: true);
        }

        [UnityTest]
        public IEnumerator Failure_Two_objects_with_the_same_save_id_make_saving_fail_cleanly()
        {
            BuildWorld();
            var clone = CreateCube("Clone", new Vector3(0f, 1f, 3f), Vector3.one * 0.5f);
            clone.AddComponent<Rigidbody>().useGravity = false;
            clone.AddComponent<SaveableProp>();   // default id "prop_01" again
            yield return null;

            Assert.IsFalse(saveSystem.Save());

            Assert.IsFalse(File.Exists(saveSystem.SavePath), "no half-valid save should be written");
            StringAssert.Contains("prop_01", saveSystem.LastMessage);
        }
    }
}
