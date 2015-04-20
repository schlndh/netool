using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Netool.Network.Tcp;
namespace Netool.Dialogs
{
    public partial class TcpServerDialog : Form
    {
        public TcpServerSettings Settings
        {
            get {
                return new TcpServerSettings { LocalEndPoint = ep, MaxConnections = int.Parse(maxConnections.Text) };
            }
        }
        private IPEndPoint ep = null;

        public TcpServerDialog()
        {
            InitializeComponent();
        }

        private void maxConnections_Validating(object sender, CancelEventArgs e)
        {
            var tb = (TextBox)sender;
            int m;
            if(!int.TryParse(tb.Text,out m) || m < 0)
            {
                e.Cancel = true;
            }
        }

        private void endPoint_Validating(object sender, CancelEventArgs e)
        {
            if(ep == null)
            {
                e.Cancel = true;
            }
        }

        private void endPointChange_Click(object sender, EventArgs e)
        {
            var dialog = new IPEndpointDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                ep = dialog.EndPoint;
                endPoint.Text = ep.ToString();
            }
        }
    }
}
