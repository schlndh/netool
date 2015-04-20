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
namespace Netool.Dialogs
{
    public partial class IPEndpointDialog : Form
    {
        public IPEndPoint EndPoint 
        {
            get {
                return new IPEndPoint(IPAddress.Parse(ipAddress.Text), int.Parse(port.Text));
            }
        }
        public IPEndpointDialog()
        {
            InitializeComponent();
        }

        private void port_Validating(object sender, CancelEventArgs e)
        {
            var tb = (TextBox)sender;
            int p;
            if (!int.TryParse(tb.Text, out p) || p < 0 || p > 65355)
            {
                e.Cancel = true;
            }
        }

        private void ipAddress_Validating(object sender, CancelEventArgs e)
        {
            var tb = (TextBox)sender;
            IPAddress ip;
            if (!IPAddress.TryParse(tb.Text, out ip)) 
            {
                e.Cancel = true;
            }
        }
    }
}
