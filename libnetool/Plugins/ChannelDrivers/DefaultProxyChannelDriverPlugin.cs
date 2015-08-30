using Netool.ChannelDrivers;
using System;
using System.Windows.Forms;

namespace Netool.Plugins.ChannelDrivers
{
    public class DefaultProxyChannelDriverPlugin : IChannelDriverPlugin
    {
        public long ID { get { return 1001; } }
        public string Name { get { return "DefaultProxyChannelDriverPlugin"; } }
        public string Description { get { return "Creates a default proxy channel driver, which only passes requests and responses."; } }
        public Version Version { get { return new Version(0, 1); } }
        public string Author { get { return "Hynek Schlindenbuch"; } }

        public ChannelDriverPack CreateChannelDriver()
        {
            var res = MessageBox.Show("Allow manual control?", "Default proxy channel driver", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Cancel) throw new SetupAbortedByUserException();
            return new ChannelDriverPack(new DefaultProxyDriver(res == DialogResult.Yes));
        }

        public ChannelDriverPack CreateChannelDriver(object settings)
        {
            try
            {
                return new ChannelDriverPack(new DefaultProxyDriver((bool)settings));
            }
            catch(InvalidCastException)
            {
                throw new InvalidSettingsTypeException();
            }
        }
    }
}
