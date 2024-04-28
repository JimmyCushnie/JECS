using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace JECS.Tests
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
        [DataRow(typeof(List<int>[]), DisplayName = "Array of generics")]
        [DataRow(typeof(List<int>[,,,,,,,]), DisplayName = "Many-dimensional array of generics")]
        [DataRow(typeof(List<CustomClass.CustomEnumWithinClass>[,,,,,,,]), DisplayName = "Many-dimensional array of generic with nested type")]
        [DataRow(typeof(List<CustomClass.CustomEnumWithinClass?>[,,,,,,,]), DisplayName = "Many-dimensional array of generic with nullable nested type")]
        [DataRow(typeof(List<Dictionary<string, int>>), DisplayName = "List of dictionary")]
        [DataRow(typeof(Dictionary<string, List<int[,,]>>), DisplayName = "Dictionary of string, list of 3D int arrays")]
        [DataRow(typeof(Tuple<string, string, List<int>, Dictionary<List<HashSet<ulong>>, Tuple<DateTime, Tuple<string, string>>>>), DisplayName = "Absolutely ridiculous mess of generics")]
        [DataRow(typeof(CustomClass), DisplayName = "Custom class")]
        [DataRow(typeof(CustomClass.CustomNestedClass), DisplayName = "Custom nested class")]
        [DataRow(typeof(CustomClass.CustomEnumWithinClass), DisplayName = "Custom enum nested within class")]
        [DataRow(typeof(CustomClass.CustomEnumWithinClass[]), DisplayName = "Array of custom enum nested within class")]
        [DataRow(typeof(HashSet<CustomClass.CustomNestedClass>), DisplayName = "Hashset of custom nested class")]
        [DataRow(typeof(int*), DisplayName = "Int pointer")]
        [DataRow(typeof(int*[]), DisplayName = "Int pointer array")]
        [DataRow(typeof(int?), DisplayName = "Nullable int")]
        [DataRow(typeof(int?[]), DisplayName = "Nullable int array")]
        public void SaveLoad_Type(System.Type SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);
    }

    class CustomClass
    {
        public class CustomNestedClass
        {
        }

        public enum CustomEnumWithinClass
        {
        }
    }
}
