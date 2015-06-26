using Netool.Network;
using Netool.Network.DataFormats;
using System.Threading;

namespace Netool.ChannelDrivers
{
    /// <summary>
    /// Dummy driver for testing purposes
    /// </summary>
    internal class DummyDriver : IChannelDriver
    {
        public string ID { get { return "DummyDriver"; } }
        public bool AllowManualControl { get { return false; } }
        public bool CanAccept(IChannel c) { return false; }

        public void Handle(IChannel c)
        {
            if(c is IServerChannel)
            {
                ((IServerChannel) c).RequestReceived += c_RequestReceived;
            }
            else if(c is IClientChannel)
            {

            }
            else if(c is IProxyChannel)
            {

            }
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