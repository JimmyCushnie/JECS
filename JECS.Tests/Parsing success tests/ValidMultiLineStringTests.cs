using System;
using System.Linq;
using JECS.MemoryFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class ValidMultiLineStringTests
    {
        [TestMethod]
        [DataRow("", DisplayName = "MLS Empty")]
        [DataRow("""


            """, DisplayName = "MLS 2 Empty Lines")]
        public void MultiLineStringSimple(string testString)
            => TestMultiLineString(testString, testString);

        [TestMethod]
        [DataRow("""
                 # Test
                 """, DisplayName = "Comment test")]
        [DataRow("""
                 "  okay lol :D  " # Test
                 # Magic potatoes!
                 Ay "Noise " # ASDF!
                 """, DisplayName = "Comment test with quotes")]
        public void MultiLineStringStrippedComments(string testString)
        {
            // This test will modify our input string to strip comments in a simple way.
            // The idea is that JECS is able to do the same by itself.
            // WARNING: No trailing / support yet!
            var strippedTestString = string.Join('\n', testString.Split('\n').Select(line =>
            {
                // First replace all # signs that would be escaped with an illegal character (tab)
                var rawLine = line.Replace("\\#", "\t");

                // Then find and remove # with everything after (removal of comments)
                var indexOfComment = rawLine.IndexOf('#');
                if (indexOfComment >= 0)
                    rawLine = rawLine[..indexOfComment];

                // Finally reverse the escaped # replacement and trim the line to get the actual data
                rawLine = rawLine.Replace("\t", "\\#").Trim();

                // Crop quotation (if any)
                if (rawLine.StartsWith('"') && rawLine.EndsWith('"'))
                    rawLine = rawLine[1..^1];

                return rawLine;
            }));

            TestMultiLineString(strippedTestString, testString);
        }

        private void TestMultiLineString(string actualExpectedTextContent, string testToWrapWithJecs)
        {
            Console.WriteLine($"Original: >>>{actualExpectedTextContent}<<<\n");

            // Base text indentation of 1, so that empty strings are supported.
            var content = string.Join('\n', testToWrapWithJecs.Split('\n').Select(line => $" {line}")) + '\n';
            var jecsRaw = $""""
                key: """
                {content} """
                """";
            Console.WriteLine($"JECS: >>>\n{jecsRaw}\n<<<\n");

            var file = new MemoryReadOnlyDataFile(jecsRaw);
            var parsedString = file.Get<string>("key");
            Console.WriteLine($"Parsed: >>>{parsedString}<<<");

            Assert.AreEqual(parsedString, actualExpectedTextContent);
        }
    }
}