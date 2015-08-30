using Netool.Network;
using Netool.Plugins;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class CreateChannelDriverDialog : Form
    {
        public IChannelDriverPlugin SelectedPlugin { get { return (pluginSelect.SelectedItems.Count > 0) ? (IChannelDriverPlugin)pluginSelect.SelectedItems[0].Tag : null; } }
        public string DriverName { get { return driverName.Text; } }

        public CreateChannelDriverDialog(Dictionary<long, IChannelDriverPlugin>.ValueCollection plugins)
        {
            InitializeComponent();
            foreach (var plugin in plugins)
            {
                var item = new ListViewItem(new string[] { plugin.Name, plugin.Version.ToString(), plugin.Author });
                item.Tag = plugin;
                this.pluginSelect.Items.Add(item);
            }
            pluginSelect.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            pluginSelect.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void instanceName_Validating(object sender, CancelEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt.Text.Length == 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(txt, "You have to enter a name of the driver.");
            }
        }

        private void protocolSelect_Validating(object sender, CancelEventArgs e)
        {
            var lv = sender as ListView;
            if (lv.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(lv, "You have to select a plugin.");
            }
        }

        private void CreateChannelDriverDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = (this.DialogResult == DialogResult.OK) && !this.ValidateChildren();
        }
    }
}