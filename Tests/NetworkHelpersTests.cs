using System;
using Xunit;
using System.Net;
using Netool.Network.Helpers;
namespace Tests
{
    public class IPEndPointParserTest
    {
        [Fact]
        public void TestValid()
        {
            IPEndPoint ep;
            ep = new IPEndPoint(IPAddress.Loopback, 0);
            Assert.Equal(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.IPv6Loopback, 1234);
            Assert.Equal(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.Parse("10.11.12.13"), 1234);
            Assert.Equal(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.Parse("2001:db8:85a3:0:0:8a2e:370:7334"), 0);
            Assert.Equal(ep, IPEndPointParser.Parse(ep.ToString()));
            ep = new IPEndPoint(IPAddress.Parse("2001::7334"), 1234);
            Assert.Equal(ep, IPEndPointParser.Parse(ep.ToString()));
        }
        [Fact]
        public void TestInvalid()
        {
            IPEndPoint ep;
            Assert.False(IPEndPointParser.TryParse(null, out ep));
            Assert.False(IPEndPointParser.TryParse("", out ep));
            Assert.False(IPEndPointParser.TryParse("132", out ep));
            Assert.False(IPEndPointParser.TryParse("300.0.0.1:1000", out ep));
            Assert.False(IPEndPointParser.TryParse("gggg::1", out ep));
            Assert.False(IPEndPointParser.TryParse("127.0.0.1:-1", out ep));
            Assert.False(IPEndPointParser.TryParse("::1:70000", out ep));
        }
    }
}
