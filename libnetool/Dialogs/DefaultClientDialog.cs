using Netool.Network.Tcp;
using Netool.Network.Udp;
using Netool.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class DefaultClientDialog : Form
    {
        public TcpClientSettings TcpSettings
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

        public UdpClientSettings UdpSettings
        {
            get
            {
                return new UdpClientSettings
                {
                    LocalEndPoint = localEndPoint.EndPoint,
                    RemoteEndPoint = remoteEndPoint.EndPoint,
                    Properties = socketSettings.Settings,
                };
            }
        }

        public DefaultClientDialog()
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

        private void DefaultClientDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK && !this.ValidateChildren())
            {
                e.Cancel = true;
            }
        }
    }
}