using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_IPAddressTests
    {
        [TestMethod]
        public void SaveLoad_IP()
        {
            IPAddress ip = IPAddress.Parse("8.8.8.8");
            TestUtilities.PerformSaveLoadTest(ip);
        }
    }
}
