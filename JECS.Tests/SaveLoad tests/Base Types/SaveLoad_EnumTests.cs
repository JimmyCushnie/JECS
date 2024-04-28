using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_EnumTests
    {
        [TestMethod]
        [DataRow(TestEnumInt.TestValue)]
        [DataRow((TestEnumInt)int.MinValue)]
        [DataRow((TestEnumInt)int.MaxValue)]
        public void SaveLoad_EnumInt(TestEnumInt SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumLong.TestValue)]
        [DataRow((TestEnumLong)long.MinValue)]
        [DataRow((TestEnumLong)long.MaxValue)]
        public void SaveLoad_EnumLong(TestEnumLong SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumShort.TestValue)]
        [DataRow((TestEnumShort)short.MinValue)]
        [DataRow((TestEnumShort)short.MaxValue)]
        public void SaveLoad_EnumShort(TestEnumShort SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumUint.TestValue)]
        [DataRow((TestEnumUint)uint.MinValue)]
        [DataRow((TestEnumUint)uint.MaxValue)]
        public void SaveLoad_EnumUint(TestEnumUint SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumUlong.TestValue)]
        [DataRow((TestEnumUlong)ulong.MinValue)]
        [DataRow((TestEnumUlong)ulong.MaxValue)]
        public void SaveLoad_EnumUlong(TestEnumUlong SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumUshort.TestValue)]
        [DataRow((TestEnumUshort)ushort.MinValue)]
        [DataRow((TestEnumUshort)ushort.MaxValue)]
        public void SaveLoad_EnumUshort(TestEnumUshort SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumByte.TestValue)]
        [DataRow((TestEnumByte)byte.MinValue)]
        [DataRow((TestEnumByte)byte.MaxValue)]
        public void SaveLoad_EnumByte(TestEnumByte SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(TestEnumSbyte.TestValue)]
        [DataRow((TestEnumSbyte)sbyte.MinValue)]
        [DataRow((TestEnumSbyte)sbyte.MaxValue)]
        public void SaveLoad_EnumSbyte(TestEnumSbyte SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);



        public enum TestEnumInt : int
        { 
            TestValue = 0
        }
        public enum TestEnumLong : long
        { 
            TestValue = 0
        }
        public enum TestEnumShort : short
        { 
            TestValue = 0
        }
        public enum TestEnumUint : uint
        { 
            TestValue = 0
        }
        public enum TestEnumUlong : ulong
        { 
            TestValue = 0
        }
        public enum TestEnumUshort : ushort
        { 
            TestValue = 0
        }
        public enum TestEnumByte : byte
        { 
            TestValue = 0
        }
        public enum TestEnumSbyte : sbyte
        { 
            TestValue = 0
        }
    }
}
