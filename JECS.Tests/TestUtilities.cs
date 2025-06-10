using Microsoft.VisualStudio.TestTools.UnitTesting;
using JECS.MemoryFiles;
using System;

namespace JECS.Tests
{
    static public class TestUtilities
    {
        public static void PerformSaveLoadTest<T>(T SAVED_VALUE)
            => PerformSaveLoadTest(SAVED_VALUE, Assert.AreEqual);

        public delegate void AssertTwoItemsAreTheSameDelegate<T>(T item1, T item2);
        public static void PerformSaveLoadTest<T>(T SAVED_VALUE, AssertTwoItemsAreTheSameDelegate<T> assertTwoItemsAreTheSameDelegate)
        {
            var file = new MemoryDataFile();

            try
            {
                const string SAVED_VALUE_KEY = "JECS_TEST_KEY";

                file.Set(SAVED_VALUE_KEY, SAVED_VALUE);
                var loadedValue = file.Get<T>(SAVED_VALUE_KEY);

                assertTwoItemsAreTheSameDelegate.Invoke(SAVED_VALUE, loadedValue);
            }
            finally
            {
                Console.WriteLine("Save under a top-level key | Contents of file:");
                Console.WriteLine("```");
                Console.WriteLine(file.GetRawText());
                Console.WriteLine("```");
            }


            // There are two ways a value can be saved: under a key, and as a whole file.
            // I want all SaveLoad tests to test both methods of saving.
            // Now ideally these would be separate tests so that it's easy to see which way of saving went wrong in the case that it's just one of them.
            // Unfortunately I can't see an easy way to do that.

            var file2 = new MemoryDataFile();
            try
            {
                file2.SaveAsObject(SAVED_VALUE);
                var loadedValue = file2.GetAsObject<T>();

                assertTwoItemsAreTheSameDelegate.Invoke(SAVED_VALUE, loadedValue);
            }
            finally
            {
                Console.WriteLine("\n\nSave as whole file | Contents of file:");
                Console.WriteLine("```");
                Console.WriteLine(file2.GetRawText());
                Console.WriteLine("```");
            }
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
                Assert.Fail($"Parsed data which should throw a {nameof(CannotRetrieveDataFromNodeException)}");
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
                Assert.Fail($"Parsed invalid file structure which should throw a {nameof(InvalidFileStructureException)}");
            }
            catch (InvalidFileStructureException expectedException)
            {
                Console.WriteLine("Got expected exception:");
                Console.WriteLine(expectedException);
            }
        }


        public static void PrintJecsLined(string jecs)
        {
            string[] lines = jecs.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                // Padding is limited to two digits. As JECS test cases are normally not that big.
                if (i < 10)
                    Console.Write(' ');
                Console.Write(i);
                Console.Write("| ");
                Console.WriteLine(lines[i]);
            }
        }
    }
}
