using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Levo.Save;

namespace Levo.Tests
{
    /// <summary>Is a piece of text a valid save? Normal, boundary and failure cases.</summary>
    public class SaveSerializerTests
    {
        static SaveEntry Entry(string id, string json) => new SaveEntry { id = id, json = json };

        static string ValidFile() =>
            SaveSerializer.ToJson("Room", new[] { Entry("player", "{\"yaw\":90}"), Entry("door_01", "{\"open\":true}") });

        // ---------- normal ----------
        [Test]
        public void Normal_Round_trip_keeps_scene_and_entries()
        {
            bool ok = SaveSerializer.TryParse(ValidFile(), out SaveData data, out string error);
            Assert.IsTrue(ok, error);
            Assert.AreEqual("Room", data.scene);
            Assert.AreEqual(SaveSerializer.CurrentVersion, data.version);
            Assert.AreEqual(2, data.entries.Count);
            Assert.AreEqual("player", data.entries[0].id);
            Assert.AreEqual("{\"open\":true}", data.entries[1].json);
        }

        // ---------- boundary ----------
        [Test]
        public void Boundary_Save_with_no_entries_is_valid()
        {
            string text = SaveSerializer.ToJson("Room", new List<SaveEntry>());
            Assert.IsTrue(SaveSerializer.TryParse(text, out SaveData data, out string error), error);
            Assert.AreEqual(0, data.entries.Count);
        }

        [Test]
        public void Boundary_State_with_quotes_and_line_breaks_round_trips()
        {
            string tricky = "{\"note\":\"she said \\\"hi\\\"\\nnew line\"}";
            string text = SaveSerializer.ToJson("Room", new[] { Entry("x", tricky) });
            Assert.IsTrue(SaveSerializer.TryParse(text, out SaveData data, out string error), error);
            Assert.AreEqual(tricky, data.entries[0].json);
        }

        // ---------- failure ----------
        [Test]
        public void Failure_Null_empty_and_blank_text_are_rejected()
        {
            foreach (string bad in new string[] { null, "", "   \n  " })
            {
                Assert.IsFalse(SaveSerializer.TryParse(bad, out SaveData data, out string error));
                Assert.IsNull(data);
                Assert.IsNotEmpty(error);
            }
        }

        [Test]
        public void Failure_Text_that_is_not_JSON_is_rejected_without_throwing()
        {
            foreach (string bad in new string[] { "hello", "{{{", "[]", "12345", "{\"version\":" })
            {
                Assert.IsFalse(SaveSerializer.TryParse(bad, out SaveData data, out string error), bad);
                Assert.IsNull(data);
                Assert.IsNotEmpty(error);
            }
        }

        [Test]
        public void Failure_Empty_object_with_no_version_is_rejected()
        {
            Assert.IsFalse(SaveSerializer.TryParse("{}", out SaveData _, out string error));
            StringAssert.Contains("version", error);
        }

        [Test]
        public void Failure_Truncated_file_is_rejected()
        {
            string text = ValidFile();
            string cut = text.Substring(0, text.Length / 2);
            Assert.IsFalse(SaveSerializer.TryParse(cut, out SaveData _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void Failure_Unsupported_version_is_rejected()
        {
            Assert.IsTrue(SaveSerializer.TryParse(ValidFile(), out SaveData data, out string _));
            data.version = SaveSerializer.CurrentVersion + 1;
            Assert.IsFalse(SaveSerializer.TryParse(JsonUtility.ToJson(data), out SaveData _, out string error));
            StringAssert.Contains("version", error);
        }

        [Test]
        public void Failure_Edited_entry_fails_the_checksum()
        {
            Assert.IsTrue(SaveSerializer.TryParse(ValidFile(), out SaveData data, out string _));
            data.entries[0].json = "{\"hacked\":true}";
            Assert.IsFalse(SaveSerializer.TryParse(JsonUtility.ToJson(data), out SaveData _, out string error));
            StringAssert.Contains("checksum", error);
        }

        [Test]
        public void Failure_Missing_checksum_is_rejected()
        {
            Assert.IsTrue(SaveSerializer.TryParse(ValidFile(), out SaveData data, out string _));
            data.checksum = "";
            Assert.IsFalse(SaveSerializer.TryParse(JsonUtility.ToJson(data), out SaveData _, out string _));
        }

        [Test]
        public void Failure_Duplicate_ids_are_rejected()
        {
            string text = SaveSerializer.ToJson("Room", new[] { Entry("a", "{}"), Entry("a", "{}") });
            Assert.IsFalse(SaveSerializer.TryParse(text, out SaveData _, out string error));
            StringAssert.Contains("twice", error);
        }

        [Test]
        public void Failure_Entry_with_empty_id_is_rejected()
        {
            string text = SaveSerializer.ToJson("Room", new[] { Entry("", "{}") });
            Assert.IsFalse(SaveSerializer.TryParse(text, out SaveData _, out string _));
        }

        [Test]
        public void Failure_Missing_scene_name_is_rejected()
        {
            string text = SaveSerializer.ToJson("", new[] { Entry("a", "{}") });
            Assert.IsFalse(SaveSerializer.TryParse(text, out SaveData _, out string _));
        }
    }
}
