using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;
using System.IO;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_FileSystemTests
    {
        const string SAVED_VALUE_KEY = "test key";


        [TestMethod]
        public void SaveLoad_DirectoryInfo()
        {
            var SAVED_VALUE = new DirectoryInfo("C:/picturesofferrets/ferretsinlittlehats/");

            var file = new MemoryDataFile();
            file.Set(SAVED_VALUE_KEY, SAVED_VALUE);
            var loadedValue = file.Get<DirectoryInfo>(SAVED_VALUE_KEY);

            Assert.AreEqual(SAVED_VALUE.FullName, loadedValue.FullName);
        }

        [TestMethod]
        public void SaveLoad_FileInfo()
        {
            var SAVED_VALUE = new FileInfo("C:/picturesofferrets/ferretsinlittlehats/ferretinalittlehat_16.png");

            var file = new MemoryDataFile();
            file.Set(SAVED_VALUE_KEY, SAVED_VALUE);
            var loadedValue = file.Get<FileInfo>(SAVED_VALUE_KEY);

            Assert.AreEqual(SAVED_VALUE.FullName, loadedValue.FullName);
        }
    }
}
