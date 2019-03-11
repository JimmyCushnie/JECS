using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class CollectionTypeTests
    {
        static DataFile file = new DataFile("tests/" + nameof(CollectionTypeTests), autoSave: false);

        [TestMethod]
        public void ListTest()
        {
            file.Set(nameof(TestList), TestList);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<List<string>>(nameof(TestList));

            CollectionAssert.AreEqual(TestList, loaded);
        }

        [TestMethod]
        public void HashSetTest()
        {
            file.Set(nameof(TestHashSet), TestHashSet);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<HashSet<string>>(nameof(TestHashSet));

            Assert.IsTrue(TestHashSet.SequenceEqual(loaded));
        }

        [TestMethod]
        public void NestedArrayTest()
        {
            file.Set(nameof(TestNestedArray), TestNestedArray);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<int[][][][]>(nameof(TestNestedArray));

            for (int i = 0; i < loaded.Length; i++)
                for (int j = 0; j < loaded[i].Length; j++)
                    for (int k = 0; k < loaded[i][j].Length; k++)
                        CollectionAssert.AreEqual(loaded[i][j][k], TestNestedArray[i][j][k]);
        }

        [TestMethod]
        public void DictionaryEasyTest()
        {
            file.Set(nameof(TestDictionaryEasy), TestDictionaryEasy);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<Dictionary<string, long>>(nameof(TestDictionaryEasy));

            Assert.IsTrue(TestDictionaryEasy.All(e => loaded.Contains(e)));
            Assert.IsTrue(loaded.All(e => TestDictionaryEasy.Contains(e)));
        }

        [TestMethod]
        public void DictionaryMediumTest()
        {
            file.Set(nameof(TestDictionaryMedium), TestDictionaryMedium);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<Dictionary<ComplexType, ComplexType>>(nameof(TestDictionaryMedium));

            Assert.IsTrue(TestDictionaryMedium.All(e => loaded.Contains(e)));
            Assert.IsTrue(loaded.All(e => TestDictionaryMedium.Contains(e)));
        }

        [TestMethod]
        public void DictionaryHardTest()
        {
            file.Set(nameof(TestDictionaryHard), TestDictionaryHard);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<Dictionary<ComplexType, (float, double[])>>(nameof(TestDictionaryHard));

            foreach (var kvp in TestDictionaryHard)
                AssertSame(kvp.Value, loaded[kvp.Key]);

            foreach (var kvp in loaded)
                AssertSame(kvp.Value, TestDictionaryHard[kvp.Key]);

            void AssertSame((float, double[]) tuple, (float, double[]) tuple2)
            {
                Assert.AreEqual(tuple.Item1, tuple2.Item1);
                CollectionAssert.AreEqual(tuple.Item2, tuple2.Item2);
            }
        }


        static readonly HashSet<string> TestHashSet = new HashSet<string>()
        {
            "Item 1",
            "item twooooooooooooo",
            "boobies",
        };

        static readonly List<string> TestList = new List<string>()
        {
            "you can't break an omelette without breaking a few eggs!",
            "PB&J",
            "Are you pregnant? Congratulations! Who's the lucky fella?",
        };

        static readonly Dictionary<string, long> TestDictionaryEasy = new Dictionary<string, long>()
        {
            ["One"] = 1, ["sixty nine lmao"] = 69, ["approximate age of the universe in Earth years"] = 13799000000
        };

        static readonly Dictionary<ComplexType, ComplexType> TestDictionaryMedium = new Dictionary<ComplexType, ComplexType>()
        {
            [new ComplexType(832, "jfhkdslfjsd", true)] = new ComplexType(22323, Environment.NewLine, false),
            [new ComplexType(int.MaxValue, "oof ouch owie my unit test", false)] = new ComplexType(int.MinValue, "penis lmao", true),
            [new ComplexType(8564698, "I like socialized healthcare", true)] = new ComplexType(99999, "aaaaaaaaaaaaaaaaa", true),
        };

        static readonly Dictionary<ComplexType, (float, double[])> TestDictionaryHard = new Dictionary<ComplexType, (float, double[])>()
        {
            [new ComplexType(99645, "ahfjkhgujkiii          ", false)] = (34.5f, new double[] { 234324, 643.754, 234.7777 }),
            [new ComplexType(90076432, "               ", true)] = (69.69f, new double[] { 1.234, 5.678, 9.101112 }),
        };

        class ComplexType
        {
            public int Integer;
            public string String;
            public bool Boolean;

            public ComplexType() { }
            public ComplexType(int integer, string text, bool boolean)
            {
                Integer = integer;
                String = text;
                Boolean = boolean;
            }

            public override bool Equals(object obj)
            {
                var other = (ComplexType)obj;
                if (other == null) return false;

                return this.Integer == other.Integer
                    && this.String == other.String
                    && this.Boolean == other.Boolean;
            }

            public override int GetHashCode()
            {
                return Integer;
            }
        }

        static readonly int[][][][] TestNestedArray = new int[][][][]
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