using System.IO;
using NUnit.Framework;
using Levo.Save;

namespace Levo.Tests
{
    /// <summary>Real file reads/writes, in a throwaway temp folder.</summary>
    public class SaveFileStoreTests
    {
        string folder;

        [SetUp]
        public void MakeFolder() =>
            folder = Path.Combine(Path.GetTempPath(), "LevoStoreTests", System.Guid.NewGuid().ToString("N"));

        [TearDown]
        public void DeleteFolder()
        {
            try { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
            catch (System.Exception) { }
            try { if (File.Exists(folder)) File.Delete(folder); }
            catch (System.Exception) { }
        }

        // ---------- normal ----------
        [Test]
        public void Normal_Written_text_reads_back_the_same()
        {
            var store = new SaveFileStore(folder, "save.json");
            Assert.IsTrue(store.TryWrite("hello save", out string werr), werr);
            Assert.IsTrue(store.Exists);
            Assert.IsTrue(store.TryRead(out string text, out string rerr), rerr);
            Assert.AreEqual("hello save", text);
        }

        [Test]
        public void Normal_Writing_again_replaces_the_old_content()
        {
            var store = new SaveFileStore(folder, "save.json");
            store.TryWrite("first", out string _);
            store.TryWrite("second", out string _);
            store.TryRead(out string text, out string _);
            Assert.AreEqual("second", text);
        }

        [Test]
        public void Normal_No_temporary_file_is_left_behind()
        {
            var store = new SaveFileStore(folder, "save.json");
            store.TryWrite("a", out string _);
            store.TryWrite("b", out string _);
            Assert.AreEqual(1, Directory.GetFiles(folder).Length);
        }

        // ---------- boundary ----------
        [Test]
        public void Boundary_Missing_folder_is_created_including_nested_folders()
        {
            var store = new SaveFileStore(Path.Combine(folder, "a", "b"), "save.json");
            Assert.IsTrue(store.TryWrite("x", out string error), error);
            Assert.IsTrue(store.Exists);
        }

        [Test]
        public void Boundary_Empty_text_can_be_written_and_read()
        {
            var store = new SaveFileStore(folder, "save.json");
            Assert.IsTrue(store.TryWrite("", out string _));
            Assert.IsTrue(store.TryRead(out string text, out string _));
            Assert.AreEqual("", text);
        }

        // ---------- failure ----------
        [Test]
        public void Failure_Reading_a_missing_file_returns_false_with_a_message()
        {
            var store = new SaveFileStore(folder, "nothing.json");
            Assert.IsFalse(store.Exists);
            Assert.IsFalse(store.TryRead(out string text, out string error));
            Assert.IsNull(text);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void Failure_Writing_when_the_folder_path_is_really_a_file_returns_false()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(folder));
            File.WriteAllText(folder, "I am a file, not a folder");
            var store = new SaveFileStore(folder, "save.json");
            Assert.IsFalse(store.TryWrite("x", out string error));
            Assert.IsNotEmpty(error);
        }
    }
}
