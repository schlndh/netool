using Netool.Network;
using Netool.Network.DataFormats;
using System.Threading;

namespace Netool.ChannelDrivers
{
    /// <summary>
    /// Dummy driver for testing purposes
    /// </summary>
    internal class DummyDriver : IServerChannelDriver
    {
        public string ID { get { return "DummyDriver"; } }
        public bool AllowManualControl { get { return false; } }
        public bool CanAccept { get { return true; } }

        public void Handle(IServerChannel c)
        {
            c.RequestReceived += c_RequestReceived;
        }

        private void c_RequestReceived(object sender, DataEventArgs e)
        {
            Thread.Sleep(500);
            var c = sender as IServerChannel;
            var data = e.Data.ToByteArray();
            if (data.Length > 1)
            {
                data[0]++;
                data[1]--;
            }
            c.Send(new ByteArray(data));
        }
    }
}