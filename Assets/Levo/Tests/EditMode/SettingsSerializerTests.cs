using NUnit.Framework;
using UnityEngine;
using Levo.Core;
using Levo.Settings;

namespace Levo.Tests
{
    public class SettingsSerializerTests
    {
        // ---------- normal ----------
        [Test]
        public void Normal_Round_trip_keeps_every_value()
        {
            var d = SettingsRules.CreateDefaults();
            d.sensitivity = 0.37f; d.volume = 0.25f;
            d.SetPath(BindingNames.Grab, "<Keyboard>/f");

            string json = SettingsSerializer.ToJson(d);
            Assert.IsTrue(SettingsSerializer.TryParse(json, out SettingsData back, out string error), error);

            Assert.AreEqual(0.37f, back.sensitivity, 0.0001f);
            Assert.AreEqual(0.25f, back.volume, 0.0001f);
            foreach (string action in BindingNames.All)
                Assert.AreEqual(d.GetPath(action), back.GetPath(action), action);
        }

        [Test]
        public void Normal_Defaults_round_trip()
        {
            string json = SettingsSerializer.ToJson(SettingsRules.CreateDefaults());
            Assert.IsTrue(SettingsSerializer.TryParse(json, out _, out string error), error);
        }

        // ---------- failure ----------
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void Failure_Empty_text_is_refused(string text)
        {
            Assert.IsFalse(SettingsSerializer.TryParse(text, out SettingsData data, out string error));
            Assert.IsNull(data);
            StringAssert.Contains("empty", error);
        }

        [Test]
        public void Failure_Not_JSON_is_refused()
        {
            Assert.IsFalse(SettingsSerializer.TryParse("this is not json", out SettingsData data, out string error));
            Assert.IsNull(data);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void Failure_Empty_object_is_refused_because_version_is_missing()
        {
            Assert.IsFalse(SettingsSerializer.TryParse("{}", out SettingsData data, out string error));
            Assert.IsNull(data);
            StringAssert.Contains("version", error);
        }

        [Test]
        public void Failure_Truncated_file_is_refused()
        {
            string json = SettingsSerializer.ToJson(SettingsRules.CreateDefaults());
            string cut = json.Substring(0, json.Length / 2);
            Assert.IsFalse(SettingsSerializer.TryParse(cut, out SettingsData data, out _));
            Assert.IsNull(data);
        }

        [Test]
        public void Failure_Hand_edited_out_of_range_value_is_refused()
        {
            var d = SettingsRules.CreateDefaults();
            string json = SettingsSerializer.ToJson(d).Replace("\"sensitivity\": 0.10000000149011612", "\"sensitivity\": 5");
            if (!json.Contains("\"sensitivity\": 5"))
            {
                d.sensitivity = 5f;                       // formatting differs: build the bad file directly
                json = JsonUtility.ToJson(d);
            }
            Assert.IsFalse(SettingsSerializer.TryParse(json, out SettingsData data, out string error));
            Assert.IsNull(data);
            StringAssert.Contains("sensitivity", error);
        }

        [Test]
        public void Failure_Reserved_key_in_the_file_is_refused()
        {
            var d = SettingsRules.CreateDefaults();
            d.SetPath(BindingNames.Jump, "<Keyboard>/w");
            Assert.IsFalse(SettingsSerializer.TryParse(JsonUtility.ToJson(d), out SettingsData data, out string error));
            Assert.IsNull(data);
            StringAssert.Contains("<Keyboard>/w", error);
        }

        [Test]
        public void Failure_Missing_action_in_the_file_is_refused()
        {
            var d = SettingsRules.CreateDefaults();
            d.bindings.RemoveAt(2);
            Assert.IsFalse(SettingsSerializer.TryParse(JsonUtility.ToJson(d), out _, out string error));
            StringAssert.Contains("no key", error);
        }
    }
}
