using Netool.ChannelDrivers;
using System;
using System.Windows.Forms;

namespace Netool.Plugins.ChannelDrivers
{
    public class CSScriptChannelDriverPlugin : IChannelDriverPlugin
    {
        public long ID { get { return 1003; } }
        public string Name { get { return "CSScriptChannelDriverPlugin"; } }
        public string Description { get { return "Creates a channel driver scriptable in C#."; } }
        public Version Version { get { return new Version(0, 1); } }
        public string Author { get { return "Hynek Schlindenbuch"; } }

        public ChannelDriverPack CreateChannelDriver()
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = "cs";
            dialog.Filter = "C# scripts |*.cs|All files|*.*";
            dialog.CheckFileExists = true;
            while(dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    return new ChannelDriverPack(new CSScriptChannelDriver(dialog.FileName));
                }
                catch(InvalidSettingsTypeException)
                {
                    if(MessageBox.Show("Unable to load given script.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                    {
                        throw new SetupAbortedByUserException();
                    }
                }
            }
            throw new SetupAbortedByUserException();
        }

        public ChannelDriverPack CreateChannelDriver(object settings)
        {
            try
            {
                return new ChannelDriverPack(new CSScriptChannelDriver((string)settings));
            }
            catch(InvalidCastException)
            {
                throw new InvalidSettingsTypeException();
            }
        }
    }
}
