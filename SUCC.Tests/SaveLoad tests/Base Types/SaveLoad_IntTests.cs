using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_IntTests
    {
        [TestMethod]
        [DataRow((sbyte)0, DisplayName = "0")]
        [DataRow((sbyte)1, DisplayName = "1")]
        [DataRow((sbyte)-1, DisplayName = "-1")]
        [DataRow(sbyte.MinValue, DisplayName = "MinValue")]
        [DataRow(sbyte.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Sbyte(sbyte SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow((byte)0, DisplayName = "0")]
        [DataRow((byte)1, DisplayName = "1")]
        [DataRow(byte.MinValue, DisplayName = "MinValue")]
        [DataRow(byte.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Byte(byte SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow((short)0, DisplayName = "0")]
        [DataRow((short)1, DisplayName = "1")]
        [DataRow((short)-1, DisplayName = "-1")]
        [DataRow(short.MinValue, DisplayName = "MinValue")]
        [DataRow(short.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Short(short SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow((ushort)0, DisplayName = "0")]
        [DataRow((ushort)1, DisplayName = "1")]
        [DataRow(ushort.MinValue, DisplayName = "MinValue")]
        [DataRow(ushort.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Ushort(ushort SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(0, DisplayName = "0")]
        [DataRow(1, DisplayName = "1")]
        [DataRow(-1, DisplayName = "-1")]
        [DataRow(int.MinValue, DisplayName = "MinValue")]
        [DataRow(int.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Int(int SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow((uint)0, DisplayName = "0")]
        [DataRow((uint)1, DisplayName = "1")]
        [DataRow(uint.MinValue, DisplayName = "MinValue")]
        [DataRow(uint.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Uint(uint SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow((long)0, DisplayName = "0")]
        [DataRow((long)1, DisplayName = "1")]
        [DataRow((long)-1, DisplayName = "-1")]
        [DataRow(long.MinValue, DisplayName = "MinValue")]
        [DataRow(long.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Long(long SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow((ulong)0, DisplayName = "0")]
        [DataRow((ulong)1, DisplayName = "1")]
        [DataRow(ulong.MinValue, DisplayName = "MinValue")]
        [DataRow(ulong.MaxValue, DisplayName = "MaxValue")]
        public void SaveLoad_Ulong(ulong SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);
    }
}
