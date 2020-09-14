using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_TypeTests
    {
        [TestMethod]
        [DataRow(typeof(int), DisplayName = "Int")]
        [DataRow(typeof(string), DisplayName = "String")]
        [DataRow(typeof(List<>), DisplayName = "Unbound list")]
        [DataRow(typeof(List<string>), DisplayName = "List of string")]
        [DataRow(typeof(List<List<string>>), DisplayName = "List of list of string")]
        [DataRow(typeof(Dictionary<,>), DisplayName = "Unbound dictionary")]
        [DataRow(typeof(Dictionary<string, int>), DisplayName = "Dictionary (string to int)")]
        [DataRow(typeof(int[]), DisplayName = "Int array")]
        [DataRow(typeof(int[,,,,,]), DisplayName = "Very high dimensional int array")]
        [DataRow(typeof(List<int>), DisplayName = "List of ints")]
        [DataRow(typeof(List<int[]>), DisplayName = "List of array of ints")]
        [DataRow(typeof(List<Dictionary<string, int>>), DisplayName = "List of dictionary")]
        [DataRow(typeof(Dictionary<string, List<int[,,]>>), DisplayName = "Dictionary of string, list of 3D int arrays")]
        [DataRow(typeof(Tuple<string, string, List<int>, Dictionary<List<HashSet<ulong>>, Tuple<DateTime, Tuple<string, string>>>>), DisplayName = "Literally insane mess of generics")]
        public void SaveLoad_Type(System.Type SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);
    }
}
