using System;
using System.Text;
using JECS.MemoryFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests.NodeAccessTests
{
    [TestClass]
    public class NodeResetTests
    {
        private const string SomeStarterJECS = """"
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
                key3: I am not in the default!
            """";

        private const string SomeDefaultJECS = """"
            top1:
                key1:
                    someOtherDefaultKey: """
                        With some text :)
                        """
                    key1:
                        key1: lol
                        defaultKey: asdf
                    key2: 1234
            top2: 8432
            top4: false # My man, speak truth!
            top3:
                key1:
                    - 123
                    - 4456
                key2: 435
            """";

        [TestMethod]
        [DataRow(SomeStarterJECS, SomeDefaultJECS, "top1.key1.someOtherDefaultKey", 7, -1, 2, 4, DisplayName = "#1: Reset something")]
        [DataRow(SomeStarterJECS, SomeDefaultJECS, "top1", 0, 6, 0, 8, DisplayName = "#2: Reset top-level")]
        [DataRow(SomeStarterJECS, SomeDefaultJECS, "top4", 14, -1, 10, 10, DisplayName = "#3: Append missing top-level")]
        [DataRow(SomeStarterJECS, SomeDefaultJECS, "top3.key3", 13, 13, 0, -1, DisplayName = "#4: Delete no-default key")]
        public void TestNodeReset(
            string inputJecs, string defaultJecs, string pathToReset,
            int startReplaceWith, int endReplaceWith,
            int startReplaceFrom, int endReplaceFrom)
        {
            Console.WriteLine("Raw:");
            TestUtilities.PrintJecsLined(inputJecs);
            var dataFile = new MemoryDataFile(inputJecs, "", defaultJecs);

            Console.WriteLine($"\nReset path: {pathToReset}");
            dataFile.ResetValueAtPathToDefault(pathToReset.Split('.'));

            string output = dataFile.GetRawText();
            Console.WriteLine("Result:");
            TestUtilities.PrintJecsLined(output);

            Console.WriteLine($"\nReplacing rows {startReplaceWith} to {endReplaceWith} replace with {startReplaceFrom} to {endReplaceFrom}");
            string expected = ReplaceRows(inputJecs, defaultJecs, startReplaceWith, endReplaceWith, startReplaceFrom, endReplaceFrom);
            Console.WriteLine("Expected:");
            TestUtilities.PrintJecsLined(expected);

            Assert.AreEqual(expected, output);
        }

        private static string ReplaceRows(string raw, string replacement,
            int startReplaceWith, int endReplaceWith,
            int startReplaceFrom, int endReplaceFrom)
        {
            string[] lines = raw.Split('\n');
            string[] replacementLines = replacement.Split('\n');
            var builder = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                // Once we encounter the very first line to replace, we inject all the replacement immediately.
                if (i == startReplaceWith)
                {
                    for (int r = startReplaceFrom; r <= endReplaceFrom; r++)
                        builder.AppendLine(replacementLines[r]);
                }

                // endReplaceWith is <0 for the no-replacement just-inject mode.
                // And in that mode injection happens before the index - weird - but this is only for this test.
                if (endReplaceWith < 0 || i < startReplaceWith || i > endReplaceWith)
                    builder.AppendLine(lines[i]);
            }
            if (lines.Length == startReplaceWith)
            {
                for (int r = startReplaceFrom; r <= endReplaceFrom; r++)
                    builder.AppendLine(replacementLines[r]);
            }
            return builder.ToString();
        }

        private const string IndentationStarterJECS = """"
            originallyWide:
             sub:
              key: value
              # Preserved for test structure.
              other: value
            originallySmall:
                    sub:
                        key: value
                        other: value
            """";

        private const string IndentationDefaultJECS = """"
            originallyWide:
                    sub:
                            key: value
             # Hey, I cannot be moved into negative :)
                            other: value
            originallySmall:
             sub:
              key: value
              other: value
            """";

        [TestMethod]
        [DataRow(IndentationStarterJECS, IndentationDefaultJECS, "originallyWide.sub", 1, 4, -7, DisplayName = "#1: Remove indentation (but not into negative)")]
        [DataRow(IndentationStarterJECS, IndentationDefaultJECS, "originallySmall.sub", 6, 8, +7, DisplayName = "#2: Add indentation")]
        public void TestNodeReset_Indentation(
            string inputJecs, string defaultJecs, string pathToReset,
            int linesStart, int linesEnd, int indentationDelta)
        {
            Console.WriteLine("Raw:");
            TestUtilities.PrintJecsLined(inputJecs);
            var dataFile = new MemoryDataFile(inputJecs, "", defaultJecs);

            Console.WriteLine($"\nReset path: {pathToReset}");
            dataFile.ResetValueAtPathToDefault(pathToReset.Split('.'));

            string output = dataFile.GetRawText();
            Console.WriteLine("Result:");
            TestUtilities.PrintJecsLined(output);

            Console.WriteLine($"\nRegion rows {linesStart} to {linesEnd} with indentation delta {indentationDelta}");
            string expected;
            {
                string[] inputLines = inputJecs.Split('\n');
                string[] defaultLines = defaultJecs.Split('\n');
                var builder = new StringBuilder();

                for (int lineIndex = 0; lineIndex < inputLines.Length; lineIndex++)
                {
                    if (lineIndex < linesStart || lineIndex > linesEnd)
                    {
                        builder.AppendLine(inputLines[lineIndex]);
                        continue;
                    }

                    string line = defaultLines[lineIndex];
                    if (indentationDelta < 0)
                    {
                        // Remove indentation
                        int innerIndentationDelta = -indentationDelta;
                        // Sadly this code is copied directly from the code which we are testing...
                        // If you know a better way, please update it.
                        int actualRemoval = 0;
                        for (int charIndex = 0; charIndex < innerIndentationDelta && charIndex < line.Length; charIndex++)
                        {
                            if (line[charIndex] != ' ')
                                break;
                            actualRemoval += 1;
                        }
                        line = line[actualRemoval..];
                    }
                    else
                    {
                        // Add indentation:
                        line = new string(' ', indentationDelta) + line;
                    }
                    builder.AppendLine(line);
                }

                expected = builder.ToString();
            }
            Console.WriteLine("Expected:");
            TestUtilities.PrintJecsLined(expected);

            Console.WriteLine("\n-prevent trim-");

            Assert.AreEqual(expected, output);
        }
    }
}