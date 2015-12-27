using Netool.Dialogs;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.DataFormats.StreamWrappers;
using System;

namespace Netool.Plugins.StreamWrappers
{
    public class ChunkedStreamWrapperPlugin : IStreamWrapperPlugin
    {
        /// <inheritdoc />
        public long ID { get { return 4003; } }

        /// <inheritdoc />
        public string Name { get { return "ChunkedStreamWrapperPlugin"; } }

        /// <inheritdoc />
        public string Description { get { return "Plugin for Dechunked Stream wrapper"; } }

        /// <inheritdoc />
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc />
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc />
        public Network.DataFormats.IStreamWrapper CreateWrapper()
        {
            var dialog = new TextBoxDialog();
            dialog.DialogTitle = "Chunked stream wrapper setup";
            dialog.DialogText = "Enter chunk size:";
            dialog.ValidatingValue += dialog_ValidatingValue;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int chunkSize = int.Parse(dialog.Value);
                return new BasicWrapper(delegate(IDataStream s) { return new ChunkedStream(s, chunkSize); }, "chunk", chunkSize);
            }
            return null;
        }

        private void dialog_ValidatingValue(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var text = (sender as System.Windows.Forms.TextBox).Text;
            int i;
            e.Cancel = !int.TryParse(text, out i) || i <= 0;
        }
    }
}