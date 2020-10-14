using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace SUCC.Tests
{
    [TestClass]
    public class SaveLoad_IPAddressTests
    {
        [TestMethod]
        public void SaveLoad_IP()
        {
            string hostName = Dns.GetHostName();
            IPAddress ip = Dns.GetHostAddresses(hostName)[0];
            TestUtilities.PerformSaveLoadTest(ip);
        }
    }
}
