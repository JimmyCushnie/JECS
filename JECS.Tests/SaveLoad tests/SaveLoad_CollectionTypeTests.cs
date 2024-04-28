using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_CollectionTypeTests
    {
        [TestMethod]
        public void SaveLoad_Array_Ints()
        {
            TestUtilities.PerformSaveLoadTest(new int[] { 0, 1, 2, 3 }, CollectionAssert.AreEqual);
        }

        [TestMethod]
        public void SaveLoad_List_Ints()
        {
            TestUtilities.PerformSaveLoadTest(new List<int>() { 0, 1, 2, 3 }, CollectionAssert.AreEqual);
        }

        [TestMethod]
        public void SaveLoad_HashSet_Ints()
        {
            TestUtilities.PerformSaveLoadTest(new HashSet<int>() { 0, 1, 2, 3 }, (a, b) => Assert.IsTrue(a.SequenceEqual(b)));
        }

        [TestMethod]
        public void SaveLoad_Array_DeeplyNestedInts()
        {
            TestUtilities.PerformSaveLoadTest(DeeplyNestedIntArray, (a, b) =>
            {
                for (int i = 0; i < a.Length; i++)
                    for (int j = 0; j < a[i].Length; j++)
                        for (int k = 0; k < a[i][j].Length; k++)
                            CollectionAssert.AreEqual(a[i][j][k], b[i][j][k]);
            });
        }

        [TestMethod]
        public void SaveLoad_Dictionary_StringToInt()
        {
            TestUtilities.PerformSaveLoadTest(new Dictionary<string, int>()
            {
                ["one"] = 1,
                ["two"] = 2,
                ["three"] = 3,
            }, CollectionAssert.AreEquivalent);
        }

        [TestMethod]
        public void SaveLoad_Dictionary_ComplexTypeToComplexType()
        {
            TestUtilities.PerformSaveLoadTest(new Dictionary<ComplexType, ComplexType>()
            {
                [new ComplexType(832, "jfhkdslfjsd", true)]                             = new ComplexType(22323, Environment.NewLine, false),
                [new ComplexType(int.MaxValue, "oof ouch owie my unit test", false)]    = new ComplexType(int.MinValue, "spicy!!!", true),
                [new ComplexType(8564698, "Hello World!", true)]                        = new ComplexType(99999, "aaaaaaaaaaaaaaaaa", true),
            }, CollectionAssert.AreEquivalent);
        }


        static readonly int[][][][] DeeplyNestedIntArray = new int[][][][]
        {
            new int[][][]
            {
                new int[][]
                {
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                    new int[]
                    {
                        110, 101, 105, 108, 99, 105, 99, 46, 99, 111, 109, 47, 109, 111, 117, 116, 104, 109, 111, 111, 100, 115
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                },
                new int[][]
                {
                    new int[]
                    {
                        104, 116, 116, 112, 58, 47, 47, 115, 117, 99, 99, 46, 115, 111, 102, 116, 119, 97, 114, 101, 47
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 119, 48, 97, 99, 110, 102, 108, 87, 114, 90, 52
                    },
                },
            },
            new int[][][]
            {
                new int[][]
                {
                    new int[]
                    {
                        115, 117, 103, 97, 110, 100, 101, 115, 101, 32, 110, 117, 116, 115, 32, 108, 109, 97, 111
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                },
                new int[][]
                {
                    new int[]
                    {
                        119, 111, 117, 108, 100, 32, 121, 111, 117, 32, 97, 99, 99, 101, 112, 116, 32, 111, 110, 101, 32, 109, 105, 108, 108, 105, 111, 110, 32, 100, 111, 108, 108, 97, 114, 115, 32, 102, 111, 114, 32, 111, 110, 101, 32, 116, 104, 111, 117, 115, 97, 110, 100, 32, 114, 97, 110, 100, 111, 109, 32, 112, 101, 111, 112, 108, 101, 32, 116, 111, 32, 100, 105, 101, 63
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                    new int[]
                    {
                        104, 116, 116, 112, 115, 58, 47, 47, 119, 119, 119, 46, 121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 119, 97, 116, 99, 104, 63, 118, 61, 100, 81, 119, 52, 119, 57, 87, 103, 88, 99, 81
                    },
                },
            }
        };
    }
}