using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Netool.Controllers;
namespace Netool.Views
{
    public partial class ProxyView : Form
    {
        private ProxyController controller;
        public ProxyView()
        {
            InitializeComponent();
        }
        public void SetController(ProxyController c)
        {
            controller = c;
        }
        public void LogMessage(string message)
        {
            log.Invoke(new Action(() => log.AppendText(message)));
        }

        private void stop_Click(object sender, EventArgs e)
        {
            controller.Stop();
        }

        private void start_Click(object sender, EventArgs e)
        {
            controller.Start();
        }
    }
}
