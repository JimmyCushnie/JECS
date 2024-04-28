using Microsoft.VisualStudio.TestTools.UnitTesting;
using JECS.MemoryFiles;
using System.Collections.Generic;

namespace JECS.Tests
{
    [TestClass]
    public class InvalidDictionaryDataTests
    {
        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_NodeHasValue()
        {
            const string fileText = @"
data: lol this isn't a dictionary
";
            TestUtilities.PerformParsingErrorTest<Dictionary<string, int>>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_HasWrongListForChildren()
        {
            const string fileText = @"
data:
    - 0
    - 1
    - 69
";
            TestUtilities.PerformParsingErrorTest<Dictionary<string, int>>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_OneInvalidValue()
        {
            const string fileText = @"
data:
    key: 12
    key2: 35
    key3: sex
";
            TestUtilities.PerformParsingErrorTest<Dictionary<string, int>>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_OneInvalidKey()
        {
            const string fileText = @"
data:
    12: value
    25: value2
    sex: value3
";
            TestUtilities.PerformParsingErrorTest<Dictionary<int, string>>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidDictionaryData_InvalidArrayDictionary()
        {
            const string fileText = @"
data:
    -
        key: sex
        value: 420
    -
        key: balls
        value: ass
";
            TestUtilities.PerformParsingErrorTest<Dictionary<string, int>>(fileText);
        }
    }
}