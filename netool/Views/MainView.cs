using Netool.Plugins;
using Netool.Controllers;
using Netool.Views;
using System;
using System.Windows.Forms;

namespace Netool
{
    public partial class MainView : Form
    {
        private MainController controller;
        private ChannelDriversView channelDrivers = new ChannelDriversView();
        public MainView()
        {
            InitializeComponent();
            AddPage("Channel Drivers", channelDrivers);
        }

        public void SetController(MainController c)
        {
            controller = c;
            channelDrivers.SetController(c);
        }

        public void AddPage(string label, Form frm)
        {
            var page = new TabPage(label);
            page.AutoScroll = true;
            page.Embed(frm);
            instances.TabPages.Add(page);
        }

        public void AddChannelDriver(int id, ChannelDriverPack pack)
        {
            channelDrivers.AddChannelDriver(id, pack);
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

        private void channelDriverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.CreateChannelDriver();
        }
    }
}