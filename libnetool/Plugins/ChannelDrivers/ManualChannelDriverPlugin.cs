using Netool.ChannelDrivers;
using System;
using System.Windows.Forms;

namespace Netool.Plugins.ChannelDrivers
{
    public class ManualChannelDriverPlugin : IChannelDriverPlugin
    {
        public long ID { get { return 1002; } }
        public string Name { get { return "ManualChannelDriverPlugin"; } }
        public string Description { get { return "Creates a manual channel driver which doesn't do anything, but lets you control the channel."; } }
        public Version Version { get { return new Version(0, 1); } }
        public string Author { get { return "Hynek Schlindenbuch"; } }

        public ChannelDriverPack CreateChannelDriver()
        {
            var dialog = new Dialogs.ManualChannelDriverSettingsDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                return new ChannelDriverPack(new ManualChannelDriver(dialog.Capacity));
            }
            else
            {
                throw new SetupAbortedByUserException();
            }
        }

        public ChannelDriverPack CreateChannelDriver(object settings)
        {
            try
            {
                return new ChannelDriverPack(new ManualChannelDriver((int)settings));
            }
            catch(InvalidCastException)
            {
                throw new InvalidSettingsTypeException();
            }
        }
    }
}
