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
    public partial class TcpClientDialog : Form
    {
        private IPEndPoint rep = null;
        private IPEndPoint lep = null;
        public TcpClientSettings Settings
        {
            get {
                return new TcpClientSettings { LocalEndPoint = lep, RemoteEndPoint = rep };
            }
        }
        public TcpClientDialog()
        {
            InitializeComponent();
        }

        private void localEndPoint_Validating(object sender, CancelEventArgs e)
        {
            if(lep == null)
            {
                e.Cancel = true;
            }
        }

        private void remoteEndPoint_Validating(object sender, CancelEventArgs e)
        {
            if(rep == null)
            {
                e.Cancel = true;
            }
        }

        private void localEndPointChange_Click(object sender, EventArgs e)
        {
            var dialog = new IPEndpointDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                lep = dialog.EndPoint;
                localEndPoint.Text = lep.ToString();
            }
        }

        private void remoteEndPointChange_Click(object sender, EventArgs e)
        {
            var dialog = new IPEndpointDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                rep = dialog.EndPoint;
                remoteEndPoint.Text = rep.ToString();
            }
        }
    }
}
