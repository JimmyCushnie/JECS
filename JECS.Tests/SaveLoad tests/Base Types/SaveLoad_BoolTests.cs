using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_BoolTests
    {
        [TestMethod]
        [DataRow(true, DisplayName = "true")]
        [DataRow(false, DisplayName = "false")]
        public void SaveLoad_Bool(bool SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);
    }
}
