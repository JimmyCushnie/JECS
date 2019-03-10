using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class DateTimeTests
    {
        static DataFile file = new DataFile("tests/" + nameof(DateTimeTests), autoSave: false);

        [TestMethod]
        public void DateTimeTest()
        {
            file.Set(nameof(TestDateTimes), TestDateTimes);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<DateTime[]>(nameof(TestDateTimes));

            CollectionAssert.AreEqual(TestDateTimes, loaded);
        }

        static readonly DateTime[] TestDateTimes = new DateTime[]
        {
            new DateTime(1961, 07, 21, 12, 20, 36),
            new DateTime(1969, 07, 21, 02, 56, 15),
            new DateTime(2011, 07, 21, 09, 57, 00),
        };
    }
}
