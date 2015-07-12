using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Netool.Dialogs;
using Netool.Controllers;
namespace Netool
{
    public partial class MainView : Form
    {
        private MainController controller;
        public MainView()
        {
            InitializeComponent();
        }
        public void SetController(MainController c)
        {
            controller = c;
        }
        public void AddPage(string label, Form frm)
        {
            var page = new TabPage(label);
            page.AutoScroll = true;
            frm.TopLevel = false;
            frm.Visible = true;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            page.Controls.Add(frm);
            instances.TabPages.Add(page);
        }
        private void MainView_Load(object sender, EventArgs e)
        {
            controller.Load();
        }

        private void serverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.CreateServer();
        }

        private void clientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.CreateClient();
        }

        private void proxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.CreateProxy();
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.Close();
        }
    }
}
