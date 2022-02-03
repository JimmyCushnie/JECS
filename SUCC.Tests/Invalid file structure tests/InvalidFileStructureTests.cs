using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;

namespace SUCC.Tests
{
    [TestClass]
    public class InvalidFileStructureTests
    {
        [TestMethod]
        public void InvalidFileStructure_NoKeyValue()
        {
            const string fileText = @"
this line doesn't have a colon to indicate key/value
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_DuplicateTopLevelKeys()
        {
            const string fileText = @"
key: value
key: value2
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_DuplicateNestedKeys()
        {
            const string fileText = @"
key:
    key2: lol
    key2: cum
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_MismatchedSiblingIndents_Back()
        {
            const string fileText = @"
key:
    key2: lol
   key3: cum
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_MismatchedSiblingIndents_Forward()
        {
            const string fileText = @"
key:
    key2: lol
     key3: cum
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_TopLevelListNodes()
        {
            const string fileText = @"
key: value
Key2: value2
key3:
    - regular
    - list

- floating list node
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }



        [TestMethod]
        public void InvalidFileStructure_MultiLineString_NoBody()
        {
            const string fileText = @"
key: """"""

key2: value
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_MultiLineString_MissingTerminator()
        {
            const string fileText = @"
key: """"""
    multi-line string
    but it doesn't have a terminator!
    fuck!

key2: value
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_MultiLineString_MismatchedBodyIndents()
        {
            const string fileText = @"
key: """"""
    multi-line string
        but the indents are screwy
    fuck!
    """"""
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }

        [TestMethod]
        public void InvalidFileStructure_MultiLineString_MismatchedTerminatorIndent()
        {
            const string fileText = @"
key: """"""
    multi-line string
        but the terminator indent is
    fuck!
       """"""
";
            TestUtilities.PerformInvalidFileStructureTest(fileText);
        }
    }
}