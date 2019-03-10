using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class FloatTests
    {
        static DataFile file = new DataFile("tests/" + nameof(FloatTests), autoSave: false);

        [TestMethod]
        public void FloatTest()
        {
            file.Set(nameof(TestFloats), TestFloats);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<float[]>(nameof(TestFloats));

            CollectionAssert.AreEqual(TestFloats, loaded);
        }

        [TestMethod]
        public void DoubleTest()
        {
            file.Set(nameof(TestDoubles), TestDoubles);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<double[]>(nameof(TestDoubles));

            CollectionAssert.AreEqual(TestDoubles, loaded);
        }

        [TestMethod]
        public void DecimalTest()
        {
            file.Set(nameof(TestDecimals), TestDecimals);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<decimal[]>(nameof(TestDecimals));

            CollectionAssert.AreEqual(TestDecimals, loaded);
        }
        

        static readonly float[] TestFloats = new float[]
        {
            // floats are tricky and can lose precision when they
            // get big or have a lot of decimal places. All of these
            // should pass the transition from data to text to data, though.
            0, 1, 2, 3, 69, -22, 5325,
            0.3333333f,
            69.6969f,
            1000000000000000000000000f,
            -1000000000000000000000000f,

            float.NegativeInfinity,
            float.PositiveInfinity,
            float.NaN,
        };

        static readonly double[] TestDoubles = new double[]
        {
            // doubles are more precise than floats but still lossy.
            // these should all pass the transition.
            0, 1, 2, 3, 69, -22, 5325,
            0.333333333333333,
            69.696969696969,
            100000000000000000000000000000000000d,
            -100000000000000000000000000000000000d,

            double.NegativeInfinity,
            double.PositiveInfinity,
            double.NaN,
        };

        static readonly decimal[] TestDecimals = new decimal[]
        {
            // to be honest, I don't really understand decimals.
            // they're like floating point values without any imprecision, I think? idk
            // anyways, these should all be saved/loaded by SUCC

            0, 1, 2, 3, 69, -22, 5325,
            0.333333333333333333333333333M,
            69.69696969696969696969696969M,

            decimal.MaxValue,
            decimal.MinValue,
        };
    }
}
