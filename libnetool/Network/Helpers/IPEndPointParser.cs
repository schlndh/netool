using System;
using System.Net;

namespace Netool.Network.Helpers
{
    public static class IPEndPointParser
    {
        public static IPEndPoint Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new FormatException("Invalid IPEndpoint string - empty string!");
            var i = str.LastIndexOf(':');
            if (i >= 0)
            {
                // IPEndPoint.ToString() wraps IPv6 addresses in []
                var ipstr = str.Substring(0, i).Replace("[", "").Replace("]", "");
                str.Substring(i + 1);
                int port;
                if (int.TryParse(str.Substring(i + 1), out port) && port >= 0 && port < 65536)
                {
                    return new IPEndPoint(IPAddress.Parse(ipstr), port);
                }
                else
                {
                    throw new FormatException("Invalid IPEndPoint string - invalid port number!");
                }
            }
            else
            {
                throw new FormatException("Invalid IPEndPoint string - no port number!");
            }
        }

        public static bool TryParse(string str, out IPEndPoint ep)
        {
            try
            {
                ep = Parse(str);
                return true;
            }
            catch (FormatException)
            {
                ep = null;
                return false;
            }
        }
    }
}