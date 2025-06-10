using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class InvalidArrayDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidArrayData_NodeHasValue()
        {
            const string fileText = """
                data: lol this isn't an int array
                """;
            TestUtilities.PerformParsingErrorTest<int[]>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidArrayData_InvalidListItemNode()
        {
            const string fileText = """
                data:
                    - 0
                    - 1
                    - cum
                    - 69
                """;
            TestUtilities.PerformParsingErrorTest<int[]>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidArrayData_InvalidNestedCollection()
        {
            const string fileText = """
                data:
                    - 22
                    - 121
                    -
                        - 55
                        - 6969
                    - 11
                """;
            TestUtilities.PerformParsingErrorTest<int[]>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidArrayData_InvalidKeyChildren()
        {
            const string fileText = """
                data:
                    child: node
                """;
            TestUtilities.PerformParsingErrorTest<int[]>(fileText);
        }
    }
}