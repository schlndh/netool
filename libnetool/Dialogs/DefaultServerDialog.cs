using Netool.Network.Tcp;
using Netool.Network.Udp;
using Netool.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class DefaultServerDialog : Form
    {
        public TcpServerSettings TcpSettings
        {
            get
            {
                return new TcpServerSettings
                {
                    LocalEndPoint = endPoint.EndPoint,
                    MaxPendingConnections = int.Parse(maxConnections.Text),
                    Properties = socketSettings.Settings,
                };
            }
        }

        public UdpServerSettings UdpSettings
        {
            get
            {
                return new UdpServerSettings
                {
                    LocalEndPoint = endPoint.EndPoint,
                    Properties = socketSettings.Settings,
                };
            }
        }

        public DefaultServerDialog()
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

        private void DefaultServerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK && !this.ValidateChildren())
            {
                e.Cancel = true;
            }
        }
    }
}