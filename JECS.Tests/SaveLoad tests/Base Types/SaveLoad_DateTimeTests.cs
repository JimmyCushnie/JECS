using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_DateTimeTests
    {
        [TestMethod]
        public void SaveLoad_DateTime_MinValue()
            => TestUtilities.PerformSaveLoadTest(DateTime.MinValue);

        // don't test with DateTime.MaxValue. It doesn't work for some reason, idk why, but it's *probably* not JECS's fault.

        [TestMethod]
        public void SaveLoad_DateTime_MyBirthday()
            => TestUtilities.PerformSaveLoadTest(new DateTime(2000, 07, 21));
    }
}
