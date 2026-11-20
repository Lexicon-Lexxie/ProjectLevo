using System;
using System.Collections.Generic;
using Levo.Core;

namespace Levo.Settings
{
    /// <summary>
    /// The rules for what a valid settings file is, plus the defaults. Pure logic (no scene, no Input System),
    /// so it is unit-tested directly.
    /// </summary>
    public static class SettingsRules
    {
        public const int CurrentVersion = 1;
        public const float MinSensitivity = 0.01f, MaxSensitivity = 1f, DefaultSensitivity = 0.1f;
        public const float MinVolume = 0f, MaxVolume = 1f, DefaultVolume = 1f;

        /// <summary>
        /// Keys a player may assign. W A S D (movement), Esc and Tab (fixed keys) are left out on purpose,
        /// so an action can never be moved onto a key that already means something else.
        /// </summary>
        public static readonly string[] AllowedPaths = BuildAllowed();

        static string[] BuildAllowed()
        {
            var list = new List<string>();
            foreach (char c in "bcefghijklmnopqrtuvxyz") list.Add("<Keyboard>/" + c);   // a-z without a, d, s, w
            for (int i = 0; i <= 9; i++) list.Add("<Keyboard>/" + i);
            list.Add("<Keyboard>/space");
            list.Add("<Keyboard>/leftShift");
            list.Add("<Keyboard>/leftCtrl");
            for (int i = 1; i <= 12; i++) list.Add("<Keyboard>/f" + i);
            return list.ToArray();
        }

        public static SettingsData CreateDefaults()
        {
            var d = new SettingsData { version = CurrentVersion, sensitivity = DefaultSensitivity, volume = DefaultVolume };
            d.SetPath(BindingNames.Jump, "<Keyboard>/space");
            d.SetPath(BindingNames.Grab, "<Keyboard>/e");
            d.SetPath(BindingNames.QuickSave, "<Keyboard>/f5");
            d.SetPath(BindingNames.QuickLoad, "<Keyboard>/f9");
            return d;
        }

        public static bool IsAllowedPath(string path) => path != null && Array.IndexOf(AllowedPaths, path) >= 0;

        static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);

        /// <summary>Never throws. True only if every value is acceptable.</summary>
        public static bool Validate(SettingsData d, out string error)
        {
            error = null;
            if (d == null) { error = "there are no settings."; return false; }
            if (d.version != CurrentVersion)
            {
                error = "unsupported settings version " + d.version + " (expected " + CurrentVersion + ").";
                return false;
            }
            if (!IsFinite(d.sensitivity) || d.sensitivity < MinSensitivity || d.sensitivity > MaxSensitivity)
            {
                error = "mouse sensitivity must be between " + MinSensitivity + " and " + MaxSensitivity + ".";
                return false;
            }
            if (!IsFinite(d.volume) || d.volume < MinVolume || d.volume > MaxVolume)
            {
                error = "volume must be between " + MinVolume + " and " + MaxVolume + ".";
                return false;
            }
            if (d.bindings == null) { error = "the key bindings are missing."; return false; }

            var seenActions = new HashSet<string>();
            var seenPaths = new HashSet<string>();
            foreach (var b in d.bindings)
            {
                if (b == null || string.IsNullOrEmpty(b.action)) { error = "a key binding has no action name."; return false; }
                if (Array.IndexOf(BindingNames.All, b.action) < 0) { error = "unknown action '" + b.action + "'."; return false; }
                if (!seenActions.Add(b.action)) { error = "action '" + b.action + "' is listed twice."; return false; }
                if (!IsAllowedPath(b.path)) { error = "'" + b.path + "' is not a key that can be assigned to '" + b.action + "'."; return false; }
                if (!seenPaths.Add(b.path)) { error = "the key '" + b.path + "' is used by two actions."; return false; }
            }
            foreach (string action in BindingNames.All)
                if (!seenActions.Contains(action)) { error = "action '" + action + "' has no key."; return false; }

            return true;
        }

        /// <summary>
        /// Next allowed key after current (direction +1) or before it (-1), skipping keys in 'used'.
        /// Returns current unchanged if nothing else is free.
        /// </summary>
        public static string NextKey(string current, int direction, ICollection<string> used)
        {
            int n = AllowedPaths.Length;
            int start = Array.IndexOf(AllowedPaths, current);
            int step = direction >= 0 ? 1 : -1;
            for (int i = 1; i <= n; i++)
            {
                int index = ((start + i * step) % n + n) % n;
                string candidate = AllowedPaths[index];
                if (candidate != current && (used == null || !used.Contains(candidate))) return candidate;
            }
            return current;
        }

        /// <summary>"&lt;Keyboard&gt;/leftShift" becomes "leftShift", for showing on screen.</summary>
        public static string Friendly(string path)
        {
            if (string.IsNullOrEmpty(path)) return "?";
            int slash = path.LastIndexOf('/');
            return slash >= 0 ? path.Substring(slash + 1) : path;
        }
    }
}
