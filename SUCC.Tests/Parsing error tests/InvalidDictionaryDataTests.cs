using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;
using System.Collections.Generic;

namespace SUCC.Tests
{
    [TestClass]
    public class InvalidDictionaryDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_NodeHasValue()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidDictionaryData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<Dictionary<string, int>>("Dictionary1");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_HasWrongListForChildren()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidDictionaryData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<Dictionary<string, int>>("Dictionary2");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_OneInvalidValue()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidDictionaryData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<Dictionary<string, int>>("Dictionary3");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_OneInvalidKey()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidDictionaryData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<Dictionary<int, string>>("Dictionary4");
            });
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_InvalidArrayDictionary()
        {
            var file = new MemoryReadOnlyDataFile(ValidFileStructure_InvalidDictionaryData);

            Assert.ThrowsException<CannotRetrieveDataFromNodeException>(() =>
            {
                file.Get<Dictionary<int, string>>("Dictionary5");
            });
        }


        const string ValidFileStructure_InvalidDictionaryData = @"

Dictionary1: lol this isn't a dictionary

Dictionary2:
    - 0
    - 1
    - 69

Dictionary3:
    key: 12
    key2: 35
    key3: sex

Dictionary4:
    12: value
    25: value2
    sex: value3

Dictionary5:
    -
        key: sex
        value: 420
    -
        key: balls
        value: ass

";
    }
}