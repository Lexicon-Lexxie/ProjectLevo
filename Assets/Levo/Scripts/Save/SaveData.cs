using System;
using System.Collections.Generic;

namespace Levo.Save
{
    /// <summary>One saved object: its id and its own JSON state.</summary>
    [Serializable]
    public class SaveEntry
    {
        public string id;
        public string json;
    }

    /// <summary>The whole save file. Version 0 (missing) is always invalid.</summary>
    [Serializable]
    public class SaveData
    {
        public int version;
        public string scene;
        public string savedAtUtc;
        public string checksum;
        public List<SaveEntry> entries = new List<SaveEntry>();
    }
}
