using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Netool.Network.Helpers;
namespace Tests
{
    [TestClass]
    public class IPEndPointParserTest
    {
        [TestMethod]
        public void TestValid()
        {
            IPEndPoint ep;
            ep = new IPEndPoint(IPAddress.Loopback, 0);
            Assert.AreEqual(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.IPv6Loopback, 1234);
            Assert.AreEqual(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.Parse("10.11.12.13"), 1234);
            Assert.AreEqual(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.Parse("2001:db8:85a3:0:0:8a2e:370:7334"), 0);
            Assert.AreEqual(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.Parse("2001::7334"), 1234);
            Assert.AreEqual(ep, IPEndPointParser.Parse(ep.ToString()));
        }
        [TestMethod]
        public void TestInvalid()
        {
            IPEndPoint ep;
            Assert.IsFalse(IPEndPointParser.TryParse(null, out ep));
            Assert.IsFalse(IPEndPointParser.TryParse("", out ep));
            Assert.IsFalse(IPEndPointParser.TryParse("132", out ep));
            Assert.IsFalse(IPEndPointParser.TryParse("300.0.0.1:1000", out ep));
            Assert.IsFalse(IPEndPointParser.TryParse("gggg::1", out ep));
            Assert.IsFalse(IPEndPointParser.TryParse("127.0.0.1:-1", out ep));
            Assert.IsFalse(IPEndPointParser.TryParse("::1:70000", out ep));
        }
    }
}
