using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_FileSystemTests
    {
        // File system infos in C# use object reference equality for their default equality comparison.
        // Therefore, the default PerformSaveLoadTest will erroneously fail, and so we substitute our own equality assertion with the full
        // path of the file system info.

        [TestMethod]
        public void SaveLoad_DirectoryInfo()
        {
            var SAVED_VALUE = new DirectoryInfo("C:/picturesofferrets/ferretsinlittlehats/");
            TestUtilities.PerformSaveLoadTest(SAVED_VALUE, (dir1, dir2) => Assert.AreEqual(dir1.FullName, dir2.FullName));
        }

        [TestMethod]
        public void SaveLoad_FileInfo()
        {
            var SAVED_VALUE = new FileInfo("C:/picturesofferrets/ferretsinlittlehats/ferretinalittlehat_16.png");
            TestUtilities.PerformSaveLoadTest(SAVED_VALUE, (file1, file2) => Assert.AreEqual(file1.FullName, file2.FullName));
        }
    }
}
