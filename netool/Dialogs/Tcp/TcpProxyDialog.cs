using Netool.Network.Tcp;
using Netool.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs.Tcp
{
    public partial class TcpProxyDialog : Form
    {
        public TcpServerSettings ServerSettings { get { return new TcpServerSettings { LocalEndPoint = localEndPoint.EndPoint, MaxPendingConnections = int.Parse(maxPendingConnetions.Text) }; } }
        public TcpClientFactorySettings ClientFactorySettings { get { return new TcpClientFactorySettings { RemoteEndPoint = remoteEndPoint.EndPoint, LocalIPAddress = localIP.IP }; } }

        public TcpProxyDialog()
        {
            InitializeComponent();
        }

        private void localIP_Validating(object sender, CancelEventArgs e)
        {
            var addr = sender as IPAddressControl;
            if (addr.IP == null)
            {
                e.Cancel = true;
            }
        }

        private void endPoint_Validating(object sender, CancelEventArgs e)
        {
            var ep = sender as IPEndPointControl;
            if (ep.EndPoint == null)
            {
                e.Cancel = true;
            }
        }

        private void maxPendingConnetions_Validating(object sender, CancelEventArgs e)
        {
            var txt = sender as TextBox;
            int max;
            if (!int.TryParse(txt.Text, out max) || max < 0)
            {
                e.Cancel = true;
            }
        }

        private void TcpProxyDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK && !this.ValidateChildren())
            {
                e.Cancel = true;
            }
        }
    }
}