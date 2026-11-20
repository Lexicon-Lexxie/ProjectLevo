using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Levo.Core;
using Levo.Player;
using Levo.Settings;

namespace Levo.Tests
{
    /// <summary>A pretend key-binding store, so these tests never need a real keyboard.</summary>
    public class FakeKeyBindings : MonoBehaviour, IKeyBindings
    {
        public readonly Dictionary<string, string> Paths = new Dictionary<string, string>();
        public string GetBinding(string action) => Paths.TryGetValue(action, out string p) ? p : null;
        public string GetDefaultBinding(string action) => null;
        public bool TrySetBinding(string action, string path) { Paths[action] = path; return true; }
    }

    /// <summary>The real SettingsManager, a real PlayerLook, a real settings file in a temp folder.</summary>
    public class SettingsManagerTests : PlayModeTestBase
    {
        string dir;
        SettingsManager manager;
        PlayerLook look;
        FakeKeyBindings keys;
        float originalVolume;

        [SetUp]
        public void Prepare()
        {
            dir = Path.Combine(Path.GetTempPath(), "LevoSettingsTests", System.Guid.NewGuid().ToString("N"));
            originalVolume = AudioListener.volume;
        }

        [TearDown]
        public void Restore()
        {
            AudioListener.volume = originalVolume;
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch (System.Exception) { }
        }

        void BuildWorld()
        {
            var player = Create("Player");
            player.SetActive(false);                              // so Awake waits until the camera pivot is set
            player.AddComponent<FakePlayerInput>();
            look = player.AddComponent<PlayerLook>();
            SetField(look, "cameraPivot", Create("Pivot").transform);
            keys = player.AddComponent<FakeKeyBindings>();
            player.SetActive(true);

            manager = Create("Settings").AddComponent<SettingsManager>();
            manager.UseDirectory(dir);
        }

        void WriteFile(string text)
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(manager.SettingsPath, text);
        }

        static SettingsData Custom()
        {
            var d = SettingsRules.CreateDefaults();
            d.sensitivity = 0.55f; d.volume = 0.4f;
            d.SetPath(BindingNames.Jump, "<Keyboard>/leftShift");
            d.SetPath(BindingNames.Grab, "<Keyboard>/f");
            return d;
        }

        // ---------- normal ----------
        [UnityTest]
        public IEnumerator Normal_No_file_uses_defaults()
        {
            BuildWorld();
            yield return null;

            Assert.AreEqual(SettingsLoadResult.NoFile, manager.Load());
            Assert.AreEqual(SettingsRules.DefaultSensitivity, look.Sensitivity, 0.0001f);
            Assert.AreEqual(1f, AudioListener.volume, 0.0001f);
            Assert.AreEqual("<Keyboard>/space", keys.GetBinding(BindingNames.Jump));
        }

        [UnityTest]
        public IEnumerator Normal_Apply_changes_the_game()
        {
            BuildWorld();
            yield return null;

            Assert.IsTrue(manager.TryApply(Custom(), out string error), error);

            Assert.AreEqual(0.55f, look.Sensitivity, 0.0001f);
            Assert.AreEqual(0.4f, AudioListener.volume, 0.0001f);
            Assert.AreEqual("<Keyboard>/leftShift", keys.GetBinding(BindingNames.Jump));
            Assert.AreEqual("<Keyboard>/f", keys.GetBinding(BindingNames.Grab));
        }

        [UnityTest]
        public IEnumerator Normal_Save_then_load_restores_the_settings()
        {
            BuildWorld();
            yield return null;
            manager.TryApply(Custom(), out _);
            Assert.IsTrue(manager.Save());
            Assert.IsTrue(File.Exists(manager.SettingsPath));

            manager.ResetToDefaults();
            Assert.AreEqual(SettingsRules.DefaultSensitivity, look.Sensitivity, 0.0001f);

            Assert.AreEqual(SettingsLoadResult.Loaded, manager.Load());
            Assert.AreEqual(0.55f, look.Sensitivity, 0.0001f);
            Assert.AreEqual(0.4f, AudioListener.volume, 0.0001f);
            Assert.AreEqual("<Keyboard>/f", keys.GetBinding(BindingNames.Grab));
        }

        [UnityTest]
        public IEnumerator Normal_Settings_survive_a_simulated_restart()
        {
            BuildWorld();
            yield return null;
            manager.TryApply(Custom(), out _);
            Assert.IsTrue(manager.Save());

            // "Close the program": destroy everything, then rebuild from scratch.
            Object.Destroy(manager.gameObject);
            Object.Destroy(look.gameObject);
            yield return null;
            yield return null;
            AudioListener.volume = 1f;

            BuildWorld();
            yield return null;
            Assert.AreEqual(SettingsLoadResult.Loaded, manager.Load());

            Assert.AreEqual(0.55f, look.Sensitivity, 0.0001f);
            Assert.AreEqual(0.4f, AudioListener.volume, 0.0001f);
            Assert.AreEqual("<Keyboard>/leftShift", keys.GetBinding(BindingNames.Jump));
        }

        [UnityTest]
        public IEnumerator Normal_Settings_file_is_separate_from_the_progress_save()
        {
            BuildWorld();
            yield return null;
            StringAssert.EndsWith("levo_settings.json", manager.SettingsPath);
        }

        // ---------- boundary ----------
        [UnityTest]
        public IEnumerator Boundary_Saving_twice_keeps_only_the_latest()
        {
            BuildWorld();
            yield return null;
            manager.TryApply(Custom(), out _);
            manager.Save();
            var second = Custom(); second.sensitivity = 0.9f;
            manager.TryApply(second, out _);
            manager.Save();

            manager.ResetToDefaults();
            manager.Load();
            Assert.AreEqual(0.9f, look.Sensitivity, 0.0001f);
        }

        // ---------- failure ----------
        [UnityTest]
        public IEnumerator Failure_Invalid_values_are_refused_and_change_nothing()
        {
            BuildWorld();
            yield return null;
            manager.TryApply(Custom(), out _);

            var bad = Custom(); bad.sensitivity = 50f;
            Assert.IsFalse(manager.TryApply(bad, out string error));
            StringAssert.Contains("sensitivity", error);

            Assert.AreEqual(0.55f, look.Sensitivity, 0.0001f);
            Assert.AreEqual(0.55f, manager.Current.sensitivity, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Failure_Garbage_file_falls_back_to_defaults_and_is_left_alone()
        {
            BuildWorld();
            yield return null;
            manager.TryApply(Custom(), out _);          // pretend the player had changed things this session
            WriteFile("garbage {{{");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Levo settings\]"));

            Assert.AreEqual(SettingsLoadResult.Invalid, manager.Load());

            Assert.AreEqual(SettingsRules.DefaultSensitivity, look.Sensitivity, 0.0001f);
            Assert.AreEqual("<Keyboard>/space", keys.GetBinding(BindingNames.Jump));
            Assert.AreEqual("garbage {{{", File.ReadAllText(manager.SettingsPath), "the bad file must not be overwritten");
            StringAssert.Contains("Using defaults", manager.LastMessage);
        }

        [UnityTest]
        public IEnumerator Failure_Out_of_range_file_falls_back_to_defaults()
        {
            BuildWorld();
            yield return null;
            var bad = Custom(); bad.volume = 7f;
            WriteFile(JsonUtility.ToJson(bad));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Levo settings\]"));

            Assert.AreEqual(SettingsLoadResult.Invalid, manager.Load());
            Assert.AreEqual(1f, AudioListener.volume, 0.0001f);
            Assert.AreEqual(SettingsRules.DefaultSensitivity, look.Sensitivity, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Failure_Writing_to_an_impossible_folder_reports_and_does_not_throw()
        {
            BuildWorld();
            yield return null;
            string blocker = Path.Combine(Path.GetTempPath(), "LevoSettingsBlocker_" + System.Guid.NewGuid().ToString("N"));
            File.WriteAllText(blocker, "i am a file, not a folder");
            try
            {
                manager.UseDirectory(Path.Combine(blocker, "sub"));
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Levo settings\]"));
                Assert.IsFalse(manager.Save());
                StringAssert.Contains("not saved", manager.LastMessage);
            }
            finally { File.Delete(blocker); }
        }

        // ---------- real key bindings ----------
        [UnityTest]
        public IEnumerator Real_PlayerInputReader_rebinds_a_key_and_rejects_unknown_actions()
        {
            var go = Create("RealInput");
            var reader = go.AddComponent<PlayerInputReader>();
            yield return null;

            Assert.AreEqual("<Keyboard>/space", reader.GetBinding(BindingNames.Jump));
            Assert.IsTrue(reader.TrySetBinding(BindingNames.Jump, "<Keyboard>/leftShift"));
            Assert.AreEqual("<Keyboard>/leftShift", reader.GetBinding(BindingNames.Jump));
            Assert.AreEqual("<Keyboard>/space", reader.GetDefaultBinding(BindingNames.Jump));

            Assert.IsFalse(reader.TrySetBinding("Fly", "<Keyboard>/f"));
            Assert.IsFalse(reader.TrySetBinding(BindingNames.Jump, ""));
            Assert.IsNull(reader.GetBinding("Fly"));
        }
    }
}
