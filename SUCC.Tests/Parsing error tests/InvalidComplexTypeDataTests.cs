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
            const string fileText = @"
data: this is an invalid shortcut lol
";
            TestUtilities.PerformParsingErrorTest<ComplexType>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_ConstructorShortcutWithInvalidData()
        {
            const string fileText = @"
data: (0, test, cum)
";
            TestUtilities.PerformParsingErrorTest<ComplexType>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_MethodShortcutWithInvalidData()
        {
            const string fileText = @"
data: MethodShortcut(cum, test, false)
";
            TestUtilities.PerformParsingErrorTest<ComplexType>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_InvalidDataInChildren()
        {
            const string fileText = @"
data:
    Integer: 69
    String: sex
    Boolean: invalid
";
            TestUtilities.PerformParsingErrorTest<ComplexType>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_ChildrenAreListNodes()
        {
            const string fileText = @"
data:
    - lol
    - it's a list lol
";
            TestUtilities.PerformParsingErrorTest<ComplexType>(fileText);
        }

        [TestMethod]
        public void InvalidFileData_InvalidComplexTypeData_ChildrenAreMultiLineString()
        {
            const string fileText = @"
data: """"""
    this is a string haha
    indeed, it's a multi-line string!
    """"""
";
            TestUtilities.PerformParsingErrorTest<ComplexType>(fileText);
        }


        const string ValidFileStructure_InvalidComplexTypeData = @"


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