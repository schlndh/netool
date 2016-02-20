using Netool.Network.Tcp;
using Netool.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs.Tcp
{
    public partial class TcpClientDialog : Form
    {
        public TcpClientSettings Settings
        {
            get
            {
                return new TcpClientSettings
                {
                    LocalEndPoint = localEndPoint.EndPoint,
                    RemoteEndPoint = remoteEndPoint.EndPoint,
                    Properties = socketSettings.Settings,
                };
            }
        }

        public TcpClientDialog()
        {
            InitializeComponent();
        }

        private void maxConnections_Validating(object sender, CancelEventArgs e)
        {
            var tb = (TextBox)sender;
            int m;
            if (!int.TryParse(tb.Text, out m) || m < 0)
            {
                e.Cancel = true;
            }
        }

        private void endPoint_Validating(object sender, CancelEventArgs e)
        {
            var c = sender as IPEndPointControl;
            if (c.EndPoint == null)
            {
                e.Cancel = true;
            }
        }

        private void TcpClientDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK && !this.ValidateChildren())
            {
                e.Cancel = true;
            }
        }
    }
}