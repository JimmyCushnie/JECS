using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;

namespace SUCC.Tests
{
    [TestClass]
    public class InvalidArrayDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidArrayData_NodeHasValue()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidArrayData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<int[]>("IntList1");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidArrayData_InvalidListItemNode()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidArrayData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<int[]>("IntList2");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidArrayData_InvalidNestedCollection()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidArrayData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<int[]>("IntList3");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidArrayData_InvalidKeyChildren()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidArrayData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<int[]>("IntList3");
            });
        }


        const string ValidFileStructure_InvalidArrayData = @"

IntList1: lol this isn't an int list

IntList2:
    - 0
    - 1
    - cum
    - 69

IntList3:
    - 22
    - 121
    -
        - 55
        - 6969
    - 11

IntList4:
    child: node

";
    }
}