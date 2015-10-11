﻿using Netool.Dialogs;
using Netool.Plugins;
using Netool.Controllers;
using Netool.Views;
using System;
using System.Windows.Forms;

namespace Netool
{
    public partial class MainView : Form
    {
        public enum SaveTempLogResult { Cancel, Yes, YesToAll, No, NoToAll };
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

        public void ShowErrorMessage(object sender, Exception e)
        {
            this.BeginInvoke(
                new Action(
                    delegate()
                    {
                        var message = string.Format("Sender: {0}\r\nException: {1}", sender, e);
                        MessageBox.Show(message, "Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                )
            );
        }

        public SaveTempLogResult ShowSaveTempLogFileDialog(string instanceName)
        {
            var dialog = new SaveTempLogFileDialog(instanceName);
            var res = dialog.ShowDialog();
            switch (res)
            {
                case DialogResult.No:
                    return dialog.ToAll ? SaveTempLogResult.NoToAll : SaveTempLogResult.No;
                case DialogResult.Yes:
                    return dialog.ToAll ? SaveTempLogResult.YesToAll : SaveTempLogResult.Yes;
                case DialogResult.Cancel:
                default:
                    return SaveTempLogResult.Cancel;
            }
        }

        public string GetLogFileTargetName(string instanceName)
        {
            logSaveDialog.Title = "Save log file for: " + instanceName;
            var res = DialogResult.None;

            while((res = logSaveDialog.ShowDialog()) == DialogResult.Cancel && MessageBox.Show("Are you sure you don't want to save the log file? It will be deleted!", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) {}
            if(res == DialogResult.OK)
            {
                return logSaveDialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}