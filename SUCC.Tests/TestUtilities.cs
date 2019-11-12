using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;

namespace SUCC.Tests
{
    static public class TestUtilities
    {
        public static void PerformSaveLoadTest<T>(T SAVED_VALUE)
        {
            const string SAVED_VALUE_KEY = "test key";
            var file = new MemoryDataFile();

            file.Set(SAVED_VALUE_KEY, SAVED_VALUE);
            var loadedValue = file.Get<T>(SAVED_VALUE_KEY);

            Assert.AreEqual(SAVED_VALUE, loadedValue);
        }
    }
}
