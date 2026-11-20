using System.Collections.Generic;
using NUnit.Framework;
using Levo.Core;
using Levo.Settings;

namespace Levo.Tests
{
    public class SettingsRulesTests
    {
        static SettingsData Good() => SettingsRules.CreateDefaults();

        static void AssertInvalid(SettingsData d, string expectedPart)
        {
            Assert.IsFalse(SettingsRules.Validate(d, out string error));
            StringAssert.Contains(expectedPart, error);
        }

        // ---------- normal ----------
        [Test]
        public void Normal_Defaults_are_valid()
        {
            Assert.IsTrue(SettingsRules.Validate(Good(), out string error), error);
        }

        [Test]
        public void Normal_Defaults_have_all_four_actions()
        {
            var d = Good();
            foreach (string action in BindingNames.All) Assert.IsNotNull(d.GetPath(action), action);
        }

        [Test]
        public void Normal_Changed_values_are_valid()
        {
            var d = Good();
            d.sensitivity = 0.5f; d.volume = 0.3f;
            d.SetPath(BindingNames.Jump, "<Keyboard>/leftShift");
            Assert.IsTrue(SettingsRules.Validate(d, out string error), error);
        }

        [Test]
        public void Normal_Clone_is_independent()
        {
            var a = Good();
            var b = a.Clone();
            b.sensitivity = 0.9f; b.SetPath(BindingNames.Grab, "<Keyboard>/f");
            Assert.AreEqual(SettingsRules.DefaultSensitivity, a.sensitivity);
            Assert.AreEqual("<Keyboard>/e", a.GetPath(BindingNames.Grab));
        }

        // ---------- boundary ----------
        [Test]
        public void Boundary_Limits_themselves_are_valid()
        {
            var d = Good(); d.sensitivity = SettingsRules.MinSensitivity; d.volume = 0f;
            Assert.IsTrue(SettingsRules.Validate(d, out string e1), e1);
            d.sensitivity = SettingsRules.MaxSensitivity; d.volume = 1f;
            Assert.IsTrue(SettingsRules.Validate(d, out string e2), e2);
        }

        [Test]
        public void Boundary_Just_outside_the_limits_is_invalid()
        {
            var d = Good(); d.sensitivity = 0.009f; AssertInvalid(d, "sensitivity");
            d = Good(); d.sensitivity = 1.01f; AssertInvalid(d, "sensitivity");
            d = Good(); d.volume = -0.01f; AssertInvalid(d, "volume");
            d = Good(); d.volume = 1.01f; AssertInvalid(d, "volume");
        }

        // ---------- failure ----------
        [Test]
        public void Failure_NaN_and_infinity_are_invalid()
        {
            var d = Good(); d.sensitivity = float.NaN; AssertInvalid(d, "sensitivity");
            d = Good(); d.volume = float.PositiveInfinity; AssertInvalid(d, "volume");
        }

        [Test]
        public void Failure_Null_and_wrong_version_are_invalid()
        {
            Assert.IsFalse(SettingsRules.Validate(null, out _));
            var d = Good(); d.version = 0; AssertInvalid(d, "version");
            d = Good(); d.version = 99; AssertInvalid(d, "version");
        }

        [Test]
        public void Failure_Unknown_action_is_invalid()
        {
            var d = Good(); d.bindings[0].action = "Fly"; AssertInvalid(d, "Fly");
        }

        [Test]
        public void Failure_Missing_action_is_invalid()
        {
            var d = Good(); d.bindings.RemoveAt(0); AssertInvalid(d, "has no key");
        }

        [Test]
        public void Failure_Action_listed_twice_is_invalid()
        {
            var d = Good(); d.bindings[1].action = d.bindings[0].action; AssertInvalid(d, "twice");
        }

        [Test]
        public void Failure_Two_actions_on_the_same_key_is_invalid()
        {
            var d = Good(); d.SetPath(BindingNames.Grab, d.GetPath(BindingNames.Jump)); AssertInvalid(d, "two actions");
        }

        [TestCase("<Keyboard>/w")]
        [TestCase("<Keyboard>/a")]
        [TestCase("<Keyboard>/escape")]
        [TestCase("<Keyboard>/tab")]
        [TestCase("<Mouse>/leftButton")]
        [TestCase("space")]
        [TestCase("")]
        [TestCase(null)]
        public void Failure_Reserved_or_malformed_keys_are_invalid(string path)
        {
            var d = Good(); d.SetPath(BindingNames.Jump, path);
            Assert.IsFalse(SettingsRules.Validate(d, out _), "path: " + path);
        }

        [Test]
        public void Failure_Missing_binding_list_is_invalid()
        {
            var d = Good(); d.bindings = null; AssertInvalid(d, "missing");
        }

        // ---------- NextKey ----------
        [Test]
        public void NextKey_moves_forward_and_back()
        {
            string first = SettingsRules.AllowedPaths[0];
            string second = SettingsRules.AllowedPaths[1];
            Assert.AreEqual(second, SettingsRules.NextKey(first, +1, null));
            Assert.AreEqual(first, SettingsRules.NextKey(second, -1, null));
        }

        [Test]
        public void NextKey_wraps_around()
        {
            string[] all = SettingsRules.AllowedPaths;
            Assert.AreEqual(all[0], SettingsRules.NextKey(all[all.Length - 1], +1, null));
            Assert.AreEqual(all[all.Length - 1], SettingsRules.NextKey(all[0], -1, null));
        }

        [Test]
        public void NextKey_skips_keys_already_used()
        {
            string[] all = SettingsRules.AllowedPaths;
            var used = new HashSet<string> { all[1], all[2] };
            Assert.AreEqual(all[3], SettingsRules.NextKey(all[0], +1, used));
        }

        [Test]
        public void NextKey_with_nothing_free_keeps_the_current_key()
        {
            string[] all = SettingsRules.AllowedPaths;
            var used = new HashSet<string>(all);
            used.Remove(all[0]);
            Assert.AreEqual(all[0], SettingsRules.NextKey(all[0], +1, used));
        }

        [Test]
        public void Allowed_keys_have_no_duplicates_and_exclude_movement_keys()
        {
            var seen = new HashSet<string>();
            foreach (string p in SettingsRules.AllowedPaths) Assert.IsTrue(seen.Add(p), "duplicate " + p);
            foreach (string k in new[] { "w", "a", "s", "d" })
                CollectionAssert.DoesNotContain(SettingsRules.AllowedPaths, "<Keyboard>/" + k);
        }

        [Test]
        public void Defaults_use_only_allowed_keys()
        {
            foreach (var b in Good().bindings) Assert.IsTrue(SettingsRules.IsAllowedPath(b.path), b.path);
        }

        [Test]
        public void Friendly_shows_just_the_key_name()
        {
            Assert.AreEqual("leftShift", SettingsRules.Friendly("<Keyboard>/leftShift"));
            Assert.AreEqual("?", SettingsRules.Friendly(null));
        }
    }
}
