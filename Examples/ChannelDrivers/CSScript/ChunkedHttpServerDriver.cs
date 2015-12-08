using Netool.ChannelDrivers;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.ChannelDrivers.CSScript
{
    /// <summary>
    /// Example channel driver which can be used as a script for CSScriptChannelDriver
    /// </summary>
    class ChunkedHttpServerDriver : IChannelDriver
    {
        public bool CanAccept(Netool.Network.IChannel c)
        {
            return c is HttpServerChannel;
        }

        public bool AllowManualControl { get { return true; } }

        public string Name { get; set; }

        public string Type { get { return "ChunkedHttpServerDriver"; } }

        public object Settings { get { return null; } }

        public void Handle(Netool.Network.IChannel c)
        {
            var channel = c as HttpServerChannel;
            if (channel == null) return;
            channel.RequestReceived += channel_RequestReceived;
        }

        void channel_RequestReceived(object sender, Netool.Network.DataEventArgs e)
        {
            var channel = sender as HttpServerChannel;
            if (channel == null) return;
            var data = new HttpData.Builder();
            data.AddHeader("Transfer-Encoding", "chunked");
            data.IsRequest = false;
            data.HttpVersion = "1.1";
            data.ReasonPhrase = "OK";
            data.StatusCode = 200;
            channel.Send(data.CreateAndClear(generateChunkedData(100 * 1024 * 1024)));
        }

        private IDataStream generateChunkedData(int dataSize)
        {
            var random = new Random();
            var buffer = new byte[512];
            var data = new List<byte>(dataSize + (dataSize / 256) * 10);
            while(dataSize > 0)
            {
                var chunkSize = Math.Min(buffer.Length, dataSize);
                dataSize -= chunkSize;
                data.AddRange(ASCIIEncoding.ASCII.GetBytes(chunkSize.ToString("x") + "\r\n"));
                random.NextBytes(buffer);
                data.AddRange(new ArraySegment<byte>(buffer, 0, chunkSize));
                data.AddRange(ASCIIEncoding.ASCII.GetBytes("\r\n"));
            }
            data.AddRange(ASCIIEncoding.ASCII.GetBytes("0\r\n\r\n"));
            return new ByteArray(data);
        }
    }
}
