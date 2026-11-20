using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Levo.Core;

namespace Levo.Save
{
    public enum LoadResult { Loaded, NoSaveFile, Invalid, WrongScene, IoError }

    /// <summary>
    /// Saves and loads every ISaveable in the scene to one JSON file.
    /// Loading is all-or-nothing: the whole file and every entry are validated BEFORE anything in the
    /// scene is changed, so a bad file can never leave the game half-restored.
    /// Entries for objects that no longer exist are ignored; saveables missing from the file keep their state.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        [SerializeField] string fileName = "levo_save.json";
        [Tooltip("Leave empty to use the standard per-user data folder (Application.persistentDataPath).")]
        [SerializeField] string directoryOverride = "";

        SaveFileStore store;

        public event Action<string> StatusChanged;
        public string LastMessage { get; private set; } = "";
        public string SavePath => Store.FilePath;

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

        /// <summary>Used by tests so they never touch the player's real save.</summary>
        public void UseDirectory(string directory)
        {
            directoryOverride = directory;
            store = null;
        }

        public bool Save()
        {
            if (!TryCollect(out Dictionary<string, ISaveable> live, out string problem))
                return Fail("Save failed: " + problem);

            var entries = new List<SaveEntry>();
            foreach (var pair in live.OrderBy(p => p.Key, StringComparer.Ordinal))
                entries.Add(new SaveEntry { id = pair.Key, json = pair.Value.CaptureState() });

            string text = SaveSerializer.ToJson(SceneManager.GetActiveScene().name, entries);
            if (!Store.TryWrite(text, out string error))
                return Fail("Save failed: could not write the file (" + error + ").");

            Report("Saved " + entries.Count + " objects.");
            return true;
        }

        public LoadResult Load()
        {
            if (!Store.Exists)
                return Fail(LoadResult.NoSaveFile, "No save file yet. Press F5 to save first.");

            if (!Store.TryRead(out string text, out string readError))
                return Fail(LoadResult.IoError, "Load failed: could not read the file (" + readError + ").");

            if (!SaveSerializer.TryParse(text, out SaveData data, out string parseError))
                return Fail(LoadResult.Invalid, "Load failed: " + parseError + " Nothing was changed.");

            if (data.scene != SceneManager.GetActiveScene().name)
                return Fail(LoadResult.WrongScene, "Load failed: this save is from scene '" + data.scene + "'. Nothing was changed.");

            if (!TryCollect(out Dictionary<string, ISaveable> live, out string problem))
                return Fail(LoadResult.Invalid, "Load failed: " + problem);

            // Phase 1: validate everything, change nothing.
            var plan = new List<KeyValuePair<ISaveable, string>>();
            foreach (var entry in data.entries)
            {
                if (!live.TryGetValue(entry.id, out ISaveable target)) continue;   // object no longer exists: ignore
                if (!target.IsValidState(entry.json))
                    return Fail(LoadResult.Invalid, "Load failed: the saved state for '" + entry.id + "' is invalid. Nothing was changed.");
                plan.Add(new KeyValuePair<ISaveable, string>(target, entry.json));
            }

            // Phase 2: apply.
            foreach (var step in plan) step.Key.RestoreState(step.Value);

            Report("Loaded " + plan.Count + " objects.");
            return LoadResult.Loaded;
        }

        static bool TryCollect(out Dictionary<string, ISaveable> live, out string problem)
        {
            live = new Dictionary<string, ISaveable>();
            problem = null;
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (!(mb is ISaveable saveable)) continue;
                if (string.IsNullOrEmpty(saveable.SaveId))
                {
                    problem = "'" + mb.name + "' has an empty Save Id.";
                    return false;
                }
                if (live.ContainsKey(saveable.SaveId))
                {
                    problem = "two objects share the Save Id '" + saveable.SaveId + "'.";
                    return false;
                }
                live.Add(saveable.SaveId, saveable);
            }
            return true;
        }

        bool Fail(string message) { Report(message, warning: true); return false; }
        LoadResult Fail(LoadResult result, string message) { Report(message, warning: true); return result; }

        void Report(string message, bool warning = false)
        {
            LastMessage = message;
            if (warning) Debug.LogWarning("[Levo save] " + message); else Debug.Log("[Levo save] " + message);
            StatusChanged?.Invoke(message);
        }
    }
}
