using System;
using System.Collections.Generic;

namespace Levo.Settings
{
    [Serializable]
    public class KeyBindingEntry
    {
        public string action;
        public string path;
    }

    /// <summary>The whole settings file. Version 0 (missing) is always invalid.</summary>
    [Serializable]
    public class SettingsData
    {
        public int version;
        public float sensitivity;
        public float volume;
        public List<KeyBindingEntry> bindings = new List<KeyBindingEntry>();

        public SettingsData Clone()
        {
            var copy = new SettingsData { version = version, sensitivity = sensitivity, volume = volume };
            foreach (var b in bindings) copy.bindings.Add(new KeyBindingEntry { action = b.action, path = b.path });
            return copy;
        }

        public string GetPath(string action)
        {
            foreach (var b in bindings) if (b.action == action) return b.path;
            return null;
        }

        public void SetPath(string action, string path)
        {
            foreach (var b in bindings) if (b.action == action) { b.path = path; return; }
            bindings.Add(new KeyBindingEntry { action = action, path = path });
        }
    }
}
