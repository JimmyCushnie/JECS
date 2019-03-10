using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class EnumTests
    {
        static DataFile file = new DataFile("tests/" + nameof(EnumTests), autoSave: false);

        [TestMethod]
        public void IntEnumTest()
        {
            file.Set(nameof(TestIntEnums), TestIntEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<IntEnum[]>(nameof(TestIntEnums));

            CollectionAssert.AreEqual(TestIntEnums, loaded);
        }

        [TestMethod]
        public void LongEnumTest()
        {
            file.Set(nameof(TestLongEnums), TestLongEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<LongEnum[]>(nameof(TestLongEnums));

            CollectionAssert.AreEqual(TestLongEnums, loaded);
        }

        [TestMethod]
        public void ShortEnumTest()
        {
            file.Set(nameof(TestShortEnums), TestShortEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<ShortEnum[]>(nameof(TestShortEnums));

            CollectionAssert.AreEqual(TestShortEnums, loaded);
        }

        [TestMethod]
        public void UintEnumTest()
        {
            file.Set(nameof(TestUintEnums), TestUintEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<UintEnum[]>(nameof(TestUintEnums));

            CollectionAssert.AreEqual(TestUintEnums, loaded);
        }

        [TestMethod]
        public void UlongEnumTest()
        {
            file.Set(nameof(TestUlongEnums), TestUlongEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<UlongEnum[]>(nameof(TestUlongEnums));

            CollectionAssert.AreEqual(TestUlongEnums, loaded);
        }

        [TestMethod]
        public void UshortEnumTest()
        {
            file.Set(nameof(TestUshortEnums), TestUshortEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<UshortEnum[]>(nameof(TestUshortEnums));

            CollectionAssert.AreEqual(TestUshortEnums, loaded);
        }

        [TestMethod]
        public void ByteEnumTest()
        {
            file.Set(nameof(TestByteEnums), TestByteEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<ByteEnum[]>(nameof(TestByteEnums));

            CollectionAssert.AreEqual(TestByteEnums, loaded);
        }

        [TestMethod]
        public void SbyteEnumTest()
        {
            file.Set(nameof(TestSbyteEnums), TestSbyteEnums);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<SbyteEnum[]>(nameof(TestSbyteEnums));

            CollectionAssert.AreEqual(TestSbyteEnums, loaded);
        }


        enum IntEnum : int { Item0, Item1, Item2, Item3, ItemMin = int.MinValue, ItemMax = int.MaxValue, }
        static readonly IntEnum[] TestIntEnums = new IntEnum[]
        {
            IntEnum.Item0, IntEnum.Item1, IntEnum.Item3, IntEnum.ItemMin, IntEnum.ItemMax, (IntEnum)300, (IntEnum)69, (IntEnum)1000000,
        };

        enum LongEnum : long { Item0, Item1, Item2, Item3, ItemMin = long.MinValue, ItemMax = long.MaxValue, }
        static readonly LongEnum[] TestLongEnums = new LongEnum[]
        {
            LongEnum.Item0, LongEnum.Item1, LongEnum.Item3, LongEnum.ItemMin, LongEnum.ItemMax, (LongEnum)300, (LongEnum)69, (LongEnum)1000000,
        };

        enum ShortEnum : short { Item0, Item1, Item2, Item3, ItemMin = short.MinValue, ItemMax = short.MaxValue, }
        static readonly ShortEnum[] TestShortEnums = new ShortEnum[]
        {
            ShortEnum.Item0, ShortEnum.Item1, ShortEnum.Item3, ShortEnum.ItemMin, ShortEnum.ItemMax, (ShortEnum)300, (ShortEnum)69, (ShortEnum)10000,
        };

        enum UintEnum : uint { Item0, Item1, Item2, Item3, ItemMin = uint.MinValue, ItemMax = uint.MaxValue, }
        static readonly UintEnum[] TestUintEnums = new UintEnum[]
        {
            UintEnum.Item0, UintEnum.Item1, UintEnum.Item3, UintEnum.ItemMin, UintEnum.ItemMax, (UintEnum)300, (UintEnum)69, (UintEnum)1000000,
        };

        enum UlongEnum : ulong { Item0, Item1, Item2, Item3, ItemMin = ulong.MinValue, ItemMax = ulong.MaxValue, }
        static readonly UlongEnum[] TestUlongEnums = new UlongEnum[]
        {
            UlongEnum.Item0, UlongEnum.Item1, UlongEnum.Item3, UlongEnum.ItemMin, UlongEnum.ItemMax, (UlongEnum)300, (UlongEnum)69, (UlongEnum)1000000,
        };

        enum UshortEnum : ushort { Item0, Item1, Item2, Item3, ItemMin = ushort.MinValue, ItemMax = ushort.MaxValue, }
        static readonly UshortEnum[] TestUshortEnums = new UshortEnum[]
        {
            UshortEnum.Item0, UshortEnum.Item1, UshortEnum.Item3, UshortEnum.ItemMin, UshortEnum.ItemMax, (UshortEnum)300, (UshortEnum)69, (UshortEnum)10000,
        };

        enum ByteEnum : byte { Item0, Item1, Item2, Item3, ItemMin = byte.MinValue, ItemMax = byte.MaxValue, }
        static readonly ByteEnum[] TestByteEnums = new ByteEnum[]
        {
            ByteEnum.Item0, ByteEnum.Item1, ByteEnum.Item3, ByteEnum.ItemMin, ByteEnum.ItemMax, (ByteEnum)99, (ByteEnum)69, (ByteEnum)100,
        };

        enum SbyteEnum : sbyte { Item0, Item1, Item2, Item3, ItemMin = sbyte.MinValue, ItemMax = sbyte.MaxValue, }
        static readonly SbyteEnum[] TestSbyteEnums = new SbyteEnum[]
        {
            SbyteEnum.Item0, SbyteEnum.Item1, SbyteEnum.Item3, SbyteEnum.ItemMin, SbyteEnum.ItemMax, (SbyteEnum)99, (SbyteEnum)69, (SbyteEnum)100,
        };
    }
}
