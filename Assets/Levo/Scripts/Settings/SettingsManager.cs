using System;
using UnityEngine;
using Levo.Core;
using Levo.Player;
using Levo.Save;

namespace Levo.Settings
{
    public enum SettingsLoadResult { Loaded, NoFile, Invalid, IoError }

    /// <summary>
    /// Owns the player's settings (mouse sensitivity, volume, keys). Loads them on start, applies them to the
    /// game, and saves them to their own file (separate from the progress save).
    /// A missing or invalid file is never fatal: the defaults are used and the bad file is left untouched.
    /// Nothing is applied unless the whole settings object validates first.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] string fileName = "levo_settings.json";
        [Tooltip("Leave empty to use the standard per-user data folder (Application.persistentDataPath).")]
        [SerializeField] string directoryOverride = "";
        [SerializeField] PlayerLook playerLook;

        SaveFileStore store;
        IKeyBindings keyBindings;

        public SettingsData Current { get; private set; } = SettingsRules.CreateDefaults();
        public string LastMessage { get; private set; } = "";
        public string SettingsPath => Store.FilePath;
        public event Action<string> StatusChanged;

        SaveFileStore Store
        {
            get
            {
                if (store == null)
                {
                    string dir = string.IsNullOrEmpty(directoryOverride) ? Application.persistentDataPath : directoryOverride;
                    store = new SaveFileStore(dir, fileName);
                }
                return store;
            }
        }

        /// <summary>Used by tests so they never touch the player's real settings.</summary>
        public void UseDirectory(string directory)
        {
            directoryOverride = directory;
            store = null;
        }

        void Start() => Load();

        void FindTargets()
        {
            if (playerLook == null) playerLook = FindFirstObjectByType<PlayerLook>();
            if (keyBindings == null)
            {
                foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                    if (mb is IKeyBindings kb) { keyBindings = kb; break; }
            }
        }

        /// <summary>Validate, then apply to the game, then remember. Returns false (and changes nothing) if invalid.</summary>
        public bool TryApply(SettingsData candidate, out string error)
        {
            if (!SettingsRules.Validate(candidate, out error)) return false;

            FindTargets();
            if (playerLook != null) playerLook.Sensitivity = candidate.sensitivity;
            AudioListener.volume = candidate.volume;
            if (keyBindings != null)
                foreach (var b in candidate.bindings) keyBindings.TrySetBinding(b.action, b.path);

            Current = candidate.Clone();
            return true;
        }

        public void ResetToDefaults()
        {
            TryApply(SettingsRules.CreateDefaults(), out _);
            Report("Settings reset to defaults (not saved until you press Save).");
        }

        public bool Save()
        {
            string text = SettingsSerializer.ToJson(Current);
            if (!Store.TryWrite(text, out string error))
            {
                Report("Settings were not saved: could not write the file (" + error + ").", warning: true);
                return false;
            }
            Report("Settings saved.");
            return true;
        }

        /// <summary>
        /// Reads the file and applies it. On any problem the defaults are applied instead and the file is left as it is.
        /// </summary>
        public SettingsLoadResult Load()
        {
            if (!Store.Exists)
            {
                TryApply(SettingsRules.CreateDefaults(), out _);
                Report("No settings file yet. Using defaults.");
                return SettingsLoadResult.NoFile;
            }

            if (!Store.TryRead(out string text, out string readError))
            {
                TryApply(SettingsRules.CreateDefaults(), out _);
                Report("Could not read the settings file (" + readError + "). Using defaults.", warning: true);
                return SettingsLoadResult.IoError;
            }

            if (!SettingsSerializer.TryParse(text, out SettingsData data, out string parseError))
            {
                TryApply(SettingsRules.CreateDefaults(), out _);
                Report(parseError + " Using defaults. The file was not changed.", warning: true);
                return SettingsLoadResult.Invalid;
            }

            TryApply(data, out _);
            Report("Settings loaded.");
            return SettingsLoadResult.Loaded;
        }

        void Report(string message, bool warning = false)
        {
            LastMessage = message;
            if (warning) Debug.LogWarning("[Levo settings] " + message); else Debug.Log("[Levo settings] " + message);
            StatusChanged?.Invoke(message);
        }
    }
}
