using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Levo.Save
{
    /// <summary>
    /// Turns a save into text and back, and decides whether text is a valid save.
    /// Pure logic with no scene access, so it is unit-tested directly.
    /// A checksum over the scene name and every entry detects truncated or hand-edited files.
    /// </summary>
    public static class SaveSerializer
    {
        public const int CurrentVersion = 1;

        public static string ToJson(string scene, IEnumerable<SaveEntry> entries)
        {
            var data = new SaveData
            {
                version = CurrentVersion,
                scene = scene,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                entries = new List<SaveEntry>(entries)
            };
            data.checksum = ComputeChecksum(data.scene, data.entries);
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>Never throws. On failure, data is null and error says why.</summary>
        public static bool TryParse(string json, out SaveData data, out string error)
        {
            data = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "The save file is empty.";
                return false;
            }

            SaveData parsed;
            try
            {
                parsed = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                error = "The save file is not valid JSON (" + e.Message + ").";
                return false;
            }

            if (parsed == null) { error = "The save file could not be read."; return false; }
            if (parsed.version != CurrentVersion)
            {
                error = "Unsupported save version " + parsed.version + " (expected " + CurrentVersion + ").";
                return false;
            }
            if (string.IsNullOrEmpty(parsed.scene)) { error = "The save file has no scene name."; return false; }
            if (parsed.entries == null) { error = "The save file has no entry list."; return false; }

            var seen = new HashSet<string>();
            foreach (var entry in parsed.entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.id)) { error = "A saved entry has no id."; return false; }
                if (entry.json == null) { error = "Entry '" + entry.id + "' has no state."; return false; }
                if (!seen.Add(entry.id)) { error = "The save file lists '" + entry.id + "' twice."; return false; }
            }

            if (string.IsNullOrEmpty(parsed.checksum) ||
                !string.Equals(parsed.checksum, ComputeChecksum(parsed.scene, parsed.entries), StringComparison.Ordinal))
            {
                error = "The save file is damaged or was edited (checksum does not match).";
                return false;
            }

            data = parsed;
            return true;
        }

        static string ComputeChecksum(string scene, IList<SaveEntry> entries)
        {
            var sb = new StringBuilder();
            sb.Append(scene).Append('\n');
            foreach (var e in entries)
                sb.Append(e.id).Append('\u001f').Append(e.json).Append('\n');

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
