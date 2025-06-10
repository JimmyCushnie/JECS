using System;
using System.Text;
using JECS.MemoryFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests.NodeAccessTests
{
    [TestClass]
    public class NodeDeletionTests
    {
        private const string SomeDefaultJECS = """"
            top1:
                key1:
                    key1:
                        key1: lol
                        key2: asdf
                  # Yes!
                    key2: 1234
            top2: 1234
               # Lel!
            top3: #awfawf
                                  
                key1: awdawf
                key2: 435
            """";

        [TestMethod]
        [DataRow(SomeDefaultJECS, "top1.key1.key1", 2, 5, DisplayName = "#1")]
        [DataRow(SomeDefaultJECS, "top3.key2", 12, 12, DisplayName = "#2")]
        [DataRow(SomeDefaultJECS, "top1.key1", 1, 6, DisplayName = "#3")]
        public void TestNodeDeletion(string input, string pathConcat, int startDeleteInclusive, int endDeleteInclusive)
        {
            Console.WriteLine("Raw:");
            TestUtilities.PrintJecsLined(input);
            var dataFile = new MemoryDataFile(input);

            Console.WriteLine($"\nDeleting keys: {pathConcat}");
            dataFile.DeleteKeyAtPath(pathConcat.Split('.'));

            string output = dataFile.GetRawText();
            Console.WriteLine("Result:");
            TestUtilities.PrintJecsLined(output);

            Console.WriteLine($"\nDeleting rows: {startDeleteInclusive} to {endDeleteInclusive} (inclusive)");
            string expected = DeleteRows(input, startDeleteInclusive, endDeleteInclusive);
            Console.WriteLine("Expected:");
            TestUtilities.PrintJecsLined(expected);

            Assert.AreEqual(expected, output);
        }

        private string DeleteRows(string raw, int startDeleteInclusive, int endDeleteInclusive)
        {
            string[] lines = raw.Split('\n');
            var builder = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i < startDeleteInclusive || i > endDeleteInclusive)
                    builder.AppendLine(lines[i]);
            }
            return builder.ToString();
        }
    }
}