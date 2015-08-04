using Netool.Controllers;
using System;
using System.Windows.Forms;

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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                do
                {
                    try
                    {
                        controller.RestoreInstance(openFileDialog.FileName);
                        return;
                    }
                    catch(Exception err) {
                        var msg = err.Message;
                    }
                } while(MessageBox.Show("Selected file couldn't be opened!", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry);

            }
        }
    }
}