using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class IntTests
    {
        static DataFile file = new DataFile("tests/" + nameof(IntTests), autoSave: false);

        [TestMethod]
        public void IntTest()
        {
            file.Set(nameof(TestInts), TestInts);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<int[]>(nameof(TestInts));
            CollectionAssert.AreEqual(TestInts, loaded);
        }

        [TestMethod]
        public void LongTest()
        {
            file.Set(nameof(TestLongs), TestLongs);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<long[]>(nameof(TestLongs));
            CollectionAssert.AreEqual(TestLongs, loaded);
        }

        [TestMethod]
        public void ShortTest()
        {
            file.Set(nameof(TestShorts), TestShorts);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<short[]>(nameof(TestShorts));
            CollectionAssert.AreEqual(TestShorts, loaded);
        }

        [TestMethod]
        public void UintTest()
        {
            file.Set(nameof(TestUints), TestUints);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<uint[]>(nameof(TestUints));
            CollectionAssert.AreEqual(TestUints, loaded);
        }

        [TestMethod]
        public void UlongTest()
        {
            file.Set(nameof(TestUlongs), TestUlongs);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<ulong[]>(nameof(TestUlongs));
            CollectionAssert.AreEqual(TestUlongs, loaded);
        }

        [TestMethod]
        public void UshortTest()
        {
            file.Set(nameof(TestUshorts), TestUshorts);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<ushort[]>(nameof(TestUshorts));
            CollectionAssert.AreEqual(TestUshorts, loaded);
        }

        [TestMethod]
        public void ByteTest()
        {
            file.Set(nameof(TestBytes), TestBytes);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<byte[]>(nameof(TestBytes));
            CollectionAssert.AreEqual(TestBytes, loaded);
        }

        [TestMethod]
        public void SbyteTest()
        {
            file.Set(nameof(TestSbytes), TestSbytes);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<sbyte[]>(nameof(TestSbytes));
            CollectionAssert.AreEqual(TestSbytes, loaded);
        }


        static readonly int[] TestInts = new int[]
        {
            0, 1, 2, 3, 69, int.MinValue, int.MaxValue, -22, 5325
        };

        static readonly long[] TestLongs = new long[]
        {
            0, 1, 2, 3, 69, long.MinValue, long.MaxValue, -22, 5325
        };

        static readonly short[] TestShorts = new short[]
        {
            0, 1, 2, 3, 69, short.MinValue, short.MaxValue, -22, 5325
        };

        static readonly uint[] TestUints = new uint[]
        {
            0, 1, 2, 3, 69, uint.MaxValue, 5325
        };

        static readonly ulong[] TestUlongs = new ulong[]
        {
            0, 1, 2, 3, 69, long.MaxValue, 5325
        };

        static readonly ushort[] TestUshorts = new ushort[]
        {
            0, 1, 2, 3, 69, ushort.MaxValue, 5325
        };

        static readonly byte[] TestBytes = new byte[]
        {
            0, byte.MaxValue, 69
        };

        static readonly sbyte[] TestSbytes = new sbyte[]
        {
            0, sbyte.MinValue, sbyte.MaxValue, 69
        };
    }
}
