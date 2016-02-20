using Netool.Network.Tcp;
using Netool.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs.Tcp
{
    public partial class TcpServerDialog : Form
    {
        public TcpServerSettings Settings
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

        public TcpServerDialog()
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

        private void TcpServerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK && !this.ValidateChildren())
            {
                e.Cancel = true;
            }
        }
    }
}