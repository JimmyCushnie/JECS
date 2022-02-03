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


        public static void PerformParsingErrorTest<TData>(string validFileStructure_invalidData)
        {
            var file = new MemoryReadOnlyDataFile(validFileStructure_invalidData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<TData>("data");
            });
        }


        public static void PerformInvalidFileStructureTest(string invalidFileStructure)
        {
            Assert.ThrowsException<InvalidFileStructureException>(() =>
            {
                var file = new MemoryReadOnlyDataFile(invalidFileStructure);
            });
        }
    }
}
