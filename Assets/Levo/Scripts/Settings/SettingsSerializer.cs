using System;
using UnityEngine;

namespace Levo.Settings
{
    /// <summary>Turns settings into text and back. Parsing also validates, so a parsed result is always usable.</summary>
    public static class SettingsSerializer
    {
        public static string ToJson(SettingsData data) => JsonUtility.ToJson(data, true);

        /// <summary>Never throws. On failure, data is null and error says why.</summary>
        public static bool TryParse(string json, out SettingsData data, out string error)
        {
            data = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json)) { error = "The settings file is empty."; return false; }

            SettingsData parsed;
            try { parsed = JsonUtility.FromJson<SettingsData>(json); }
            catch (Exception e) { error = "The settings file is not valid JSON (" + e.Message + ")."; return false; }

            if (parsed == null) { error = "The settings file could not be read."; return false; }
            if (!SettingsRules.Validate(parsed, out string why)) { error = "The settings file is invalid: " + why; return false; }

            data = parsed;
            return true;
        }
    }
}
