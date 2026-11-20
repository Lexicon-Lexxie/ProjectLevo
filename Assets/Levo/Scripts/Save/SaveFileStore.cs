using System;
using System.IO;

namespace Levo.Save
{
    /// <summary>
    /// Reads and writes one text file. Writes go to a temporary file first and then replace the real
    /// file, so a crash mid-write can never leave a half-written save. Never throws; returns false + message.
    /// </summary>
    public sealed class SaveFileStore
    {
        public string FolderPath { get; }
        public string FilePath { get; }

        public SaveFileStore(string folderPath, string fileName)
        {
            FolderPath = folderPath;
            FilePath = Path.Combine(folderPath, fileName);
        }

        public bool Exists => File.Exists(FilePath);

        public bool TryWrite(string text, out string error)
        {
            error = null;
            string temp = FilePath + ".tmp";
            try
            {
                Directory.CreateDirectory(FolderPath);
                File.WriteAllText(temp, text);
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(temp, FilePath);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                try { if (File.Exists(temp)) File.Delete(temp); } catch (Exception) { }
                return false;
            }
        }

        public bool TryRead(out string text, out string error)
        {
            text = null;
            error = null;
            try
            {
                text = File.ReadAllText(FilePath);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }
    }
}
