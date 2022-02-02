using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;

namespace SUCC.Tests
{
    [TestClass]
    public class InvalidBaseTypeNodeDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidBaseType_InvalidNodeValue()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidIntData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<int>("IntValue");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidBaseType_BadSpecialStringCase()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidIntData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<int>("IntValue2");
            });
        }

        const string ValidFileStructure_InvalidIntData = @"

IntValue: notanint

IntValue2: """"""
    lmao it's a string
    """"""

# this file also has a comment because why not

";
    }
}