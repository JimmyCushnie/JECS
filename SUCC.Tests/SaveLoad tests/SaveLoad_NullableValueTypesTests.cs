using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_NullableValueTypesTests
    {
        [TestMethod]
        [DataRow(0, DisplayName = "Zero")]
        [DataRow(643543, DisplayName = "Arbitrary number")]
        [DataRow(null, DisplayName = "Null")]
        public void SaveLoad_NullableInt(int? SAVED_VALUE)
        {
            TestUtilities.PerformSaveLoadTest(SAVED_VALUE);
        }

        [TestMethod]
        public void SaveLoad_NullableIntArray()
        {
            int?[] SAVED_VALUE =
            [
                null, null, 432, -5423532, 43242, null, 777777, 88888, null
            ];

            TestUtilities.PerformSaveLoadTest(SAVED_VALUE, CollectionAssert.AreEqual);
        }
    }
}
