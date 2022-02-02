using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;
using System.Collections.Generic;

namespace SUCC.Tests
{
    [TestClass]
    public class InvalidComplexTypeDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_InvalidShortcut()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidComplexTypeData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<ComplexType>("ComplexType1");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_ConstructorShortcutWithInvalidData()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidComplexTypeData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<ComplexType>("ComplexType2");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_MethodShortcutWithInvalidData()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidComplexTypeData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<ComplexType>("ComplexType3");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_InvalidDataInChildren()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidComplexTypeData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<ComplexType>("ComplexType4");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_ChildrenAreListNodes()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidComplexTypeData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<ComplexType>("ComplexType5");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_ChildrenAreMultiLineString()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidComplexTypeData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<ComplexType>("ComplexType6");
            });
        }


        const string ValidFileStructure_InvalidComplexTypeData = @"

ComplexType1: this is an invalid shortcut lol
ComplexType2: (0, test, cum)
ComplexType3: MethodShortcut(cum, test, false)

ComplexType4:
    Integer: 69
    String: sex
    Boolean: invalid

ComplexType5:
    - lol
    - it's a list lol

ComplexType6: """"""
    this is a string haha
    indeed, it's a multi-line string!
    """"""

";
    }
}