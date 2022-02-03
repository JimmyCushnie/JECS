using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;
using System;

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

            Console.WriteLine($"Successfully saved and loaded {SAVED_VALUE}");
        }


        public static void PerformParsingErrorTest<TData>(string validFileStructure_invalidData)
        {
            var file = new MemoryReadOnlyDataFile(validFileStructure_invalidData);

            // Use try/catch instead of Assert.ThrowsException so we can print the exception details.
            // The point of these tests is to make sure the exceptions are working properly, so it's nice to
            // manually check that the error messages are what they should be.
            // It might be cool in the future to actually test that the error messages, or at least the line numbers, are correct.
            try
            {
                file.Get<TData>("data");
            }
            catch (CannotRetrieveDataFromNodeException expectedException)
            {
                Console.WriteLine("Got expected exception:");
                Console.WriteLine(expectedException);
            }
        }


        public static void PerformInvalidFileStructureTest(string invalidFileStructure)
        {
            try
            {
                var file = new MemoryReadOnlyDataFile(invalidFileStructure);
            }
            catch (InvalidFileStructureException expectedException)
            {
                Console.WriteLine("Got expected exception:");
                Console.WriteLine(expectedException);
            }
        }
    }
}
