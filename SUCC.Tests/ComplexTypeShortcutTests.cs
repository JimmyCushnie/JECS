using Microsoft.VisualStudio.TestTools.UnitTesting;
using SUCC.MemoryFiles;

namespace SUCC.Tests
{
    [TestClass]
    public class ComplexTypeShortcutTests
    {
        const string SAVED_VALUE_KEY = "test key";

        [TestMethod]
        public void ComplexTypeShortcut_Property_LoadedValueEqualsShortcutValue()
        {
            var SAVED_VALUE = ComplexType.PropertyShortcut;
            var file = new MemoryReadOnlyDataFile($"{SAVED_VALUE_KEY}:PropertyShortcut");

            var loadedValue = file.Get<ComplexType>(SAVED_VALUE_KEY, null);

            Assert.AreEqual(SAVED_VALUE, loadedValue);
        }

        [TestMethod]
        public void ComplexTypeShortcut_Constructor_LoadedValueEqualsShortcutValue()
        {
            var SAVED_VALUE = new ComplexType(0, "example", true);
            var file = new MemoryDataFile($"{SAVED_VALUE_KEY}:(0, \"example\", true)");

            var loadedValue = file.Get<ComplexType>(SAVED_VALUE_KEY);

            Assert.AreEqual(SAVED_VALUE, loadedValue);
        }

        [TestMethod]
        public void ComplexTypeShortcut_Method_LoadedValueEqualsShortcutValue()
        {
            var SAVED_VALUE = ComplexType.MethodShortcut(1, "test", false);
            var file = new MemoryDataFile($"{SAVED_VALUE_KEY}:MethodShortcut(1, \"test\", false)");

            var loadedValue = file.Get<ComplexType>(SAVED_VALUE_KEY);

            Assert.AreEqual(SAVED_VALUE, loadedValue);
        }

        [TestMethod]
        public void ComplexTypeShortcut_Custom_LoadedValueEqualsShortcutValue()
        {
            var SAVED_VALUE = ComplexType.Shortcut("shortcut1");
            var file = new MemoryDataFile($"{SAVED_VALUE_KEY}:shortcut1");

            var loadedValue = file.Get<ComplexType>(SAVED_VALUE_KEY);

            Assert.AreEqual(SAVED_VALUE, loadedValue);
        }
    }
}