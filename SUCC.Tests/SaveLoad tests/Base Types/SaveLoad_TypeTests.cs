using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_TypeTests
    {
        [TestMethod]
        [DataRow(typeof(int), DisplayName = "Int")]
        [DataRow(typeof(string), DisplayName = "String")]
        [DataRow(typeof(int[]), DisplayName = "Int array")]
        [DataRow(typeof(List<int>), DisplayName = "List of ints")]
        [DataRow(typeof(List<int[]>), DisplayName = "List of array of ints")]
        [DataRow(typeof(List<Dictionary<string, int>>), DisplayName = "List of dictionary")]
        public void SaveLoad_Type(System.Type SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);
    }
}
