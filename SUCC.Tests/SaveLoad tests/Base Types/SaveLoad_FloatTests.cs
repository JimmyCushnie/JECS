using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_FloatTests
    {
        // We don't use MinValue and MaxValue with float and double because floats are very
        // imprecise at that magnitude and the test will fail even though the functionality works.
        // The values listed as "large positive" and "large negative" are within the precision domain.

        [TestMethod]
        [DataRow(0f, DisplayName = "0")]
        [DataRow(1f, DisplayName = "1")]
        [DataRow(-1f, DisplayName = "-1")]
        [DataRow(69.6969f, DisplayName = "fractional value")]
        [DataRow(1000000000000000000000000f, DisplayName = "large positive")]
        [DataRow(-1000000000000000000000000f, DisplayName = "large negative")]
        [DataRow(float.NegativeInfinity, DisplayName = "negative infinity")]
        [DataRow(float.PositiveInfinity, DisplayName = "positive infinity")]
        [DataRow(float.NaN, DisplayName = "NaN")]
        public void SaveLoad_Float(float SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        [TestMethod]
        [DataRow(0d, DisplayName = "0")]
        [DataRow(1d, DisplayName = "1")]
        [DataRow(-1d, DisplayName = "-1")]
        [DataRow(69.6969d, DisplayName = "fractional value")]
        [DataRow(1000000000000000000000000d, DisplayName = "large positive")]
        [DataRow(-1000000000000000000000000d, DisplayName = "large negative")]
        [DataRow(double.NegativeInfinity, DisplayName = "negative infinity")]
        [DataRow(double.PositiveInfinity, DisplayName = "positive infinity")]
        [DataRow(double.NaN, DisplayName = "NaN")]
        public void SaveLoad_Double(double SAVED_VALUE)
            => TestUtilities.PerformSaveLoadTest(SAVED_VALUE);

        // decimals cannot be used with DataRow. https://stackoverflow.com/questions/507528/use-decimal-values-as-attribute-params-in-c
        [TestMethod]
        public void SaveLoad_Decimal_0()
            => TestUtilities.PerformSaveLoadTest(0m);
        [TestMethod]
        public void SaveLoad_Decimal_1()
            => TestUtilities.PerformSaveLoadTest(1m);
        [TestMethod]
        public void SaveLoad_Decimal_minus1()
            => TestUtilities.PerformSaveLoadTest(-1m);
        [TestMethod]
        public void SaveLoad_Decimal_FractionalValue()
            => TestUtilities.PerformSaveLoadTest(69.69696969696969696969696969m);
        [TestMethod]
        public void SaveLoad_Decimal_MinValue()
            => TestUtilities.PerformSaveLoadTest(decimal.MinValue);
        [TestMethod]
        public void SaveLoad_Decimal_MaxValue()
            => TestUtilities.PerformSaveLoadTest(decimal.MaxValue);
    }
}
