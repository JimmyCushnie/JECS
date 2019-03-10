using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class BoolTests
    {
        static DataFile file = new DataFile("tests/" + nameof(BoolTests), autoSave: false);

        [TestMethod]
        public void BoolTest()
        {
            file.Set(nameof(TestBools), TestBools);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<bool[]>(nameof(TestBools));

            CollectionAssert.AreEqual(TestBools, loaded);
        }

        static readonly bool[] TestBools = new bool[]
        {
            false, true, true, false, true, false, false, false, false, true, true, true, false, true, false, false, false, true, true, true, false, true, false, false, false, true, true, true, false, false, false, false, false, true, true, true, false, false, true, true, false, false, true, true, true, false, true, false, false, false, true, false, true, true, true, true, false, false, true, false, true, true, true, true, false, true, true, true, false, true, true, true, false, true, true, true, false, true, true, true, false, true, true, true, false, true, true, true, false, false, true, false, true, true, true, false, false, true, true, true, true, false, false, true, false, true, true, false, true, true, true, true, false, true, true, true, false, true, false, true, false, true, true, true, false, true, false, false, false, true, true, true, false, true, false, true, false, true, true, false, false, false, true, false, false, true, true, false, false, true, false, true, false, false, true, false, true, true, true, false, false, true, true, false, false, false, true, true, false, true, true, false, true, true, true, true, false, true, true, false, true, true, false, true, false, false, true, false, true, true, true, true, false, true, true, true, false, true, true, true, false, true, true, false, false, false, false, true, false, true, true, true, false, true, false, false, false, true, true, false, false, false, true, true, false, true, true, false, true, false, false, false, false, false, true, true, true, true, true, true, false, true, true, true, false, true, true, false, false, false, true, true, true, true, false, true, false, true, true, false, false, true, false, false, false, true, false, true, false, false, false, true, false, true, true, true, false, true, true, true, false, false, true, true, false, true, false, false, false, true, true, true, false, true, true, true, false, false, true, true, true, false, false, true, false, true, false, true, false, true, true, true, false, true, true, false, false, true, true, true, false, true, false, true, true, false, false, false, false, true, true, false, false, false, true, true, false, true, false, true, false, false, false, true,
        };
    }
}
