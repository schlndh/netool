using Netool.Dialogs.StreamWrappers;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.StreamWrappers;
using System;

namespace Netool.Plugins.StreamWrappers
{
    public class StreamSegmentWrapperPlugin : IStreamWrapperPlugin
    {
        /// <inheritdoc />
        public long ID { get { return 4002; } }

        /// <inheritdoc />
        public string Name { get { return "StreamSegmentWrapperPlugin"; } }

        /// <inheritdoc />
        public string Description { get { return "Plugin for StreamSegment wrapper"; } }

        /// <inheritdoc />
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc />
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc />
        public Network.DataFormats.IStreamWrapper CreateWrapper()
        {
            var dialog = new StreamSegmentSetupDialog();
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                long off = dialog.Offset;
                long count = dialog.Count;
                return new BasicWrapper(delegate(IDataStream s) { return new StreamSegment(s, off, count); }, "segment", off, count);
            }
            return null;
        }
    }
}
