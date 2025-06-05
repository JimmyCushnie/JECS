using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class InvalidBaseTypeNodeDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidBaseType_InvalidNodeValue()
        {
            const string fileText = """
                data: notanint
                """;
            TestUtilities.PerformParsingErrorTest<int>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidBaseType_BadSpecialStringCase()
        {
            const string fileText = """"
                data: """
                    lmao it's a string
                    """
                """";
            TestUtilities.PerformParsingErrorTest<int>(fileText);
        }
    }
}