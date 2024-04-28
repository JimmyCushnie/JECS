using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_ComplexTypeTests
    {
        [TestMethod]
        public void SaveLoad_ComplexType()
            => TestUtilities.PerformSaveLoadTest(new ComplexType(69, "lychee is a good fruit", true));

        [TestMethod]
        public void SaveLoad_ComplexType_Null()
            => TestUtilities.PerformSaveLoadTest<ComplexType>(null);
    }
}