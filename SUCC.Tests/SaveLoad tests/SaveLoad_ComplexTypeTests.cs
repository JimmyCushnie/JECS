using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_ComplexTypeTests
    {
        [TestMethod]
        public void SaveLoad_ComplexType()
            => TestUtilities.PerformSaveLoadTest(new ComplexType(69, "sugandese nuts lmao", true));

        [TestMethod]
        public void SaveLoad_ComplexType_Null()
            => TestUtilities.PerformSaveLoadTest<ComplexType>(null);
    }
}