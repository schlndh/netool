using Netool.Network;
using Netool.Plugins;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class CreateInstanceDialog : Form
    {
        public IProtocolPlugin SelectedPlugin { get { return (protocolSelect.SelectedItems.Count > 0) ? (IProtocolPlugin)protocolSelect.SelectedItems[0].Tag : null; } }
        public string InstanceName { get { return this.instanceName.Text; } }

        public CreateInstanceDialog(List<IProtocolPlugin> protocols, InstanceType type)
        {
            InitializeComponent();
            foreach (var protocol in protocols)
            {
                if (!protocol.SupportsType(type)) continue;
                var item = new ListViewItem(new string[] { protocol.ProtocolName, protocol.Name, protocol.Version.ToString(), protocol.Author });
                item.Tag = protocol;
                this.protocolSelect.Items.Add(item);
            }
            protocolSelect.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            protocolSelect.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void instanceName_Validating(object sender, CancelEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt.Text.Length == 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(txt, "You have to enter a name of the instance.");
            }
        }

        private void protocolSelect_Validating(object sender, CancelEventArgs e)
        {
            var lv = sender as ListView;
            if (lv.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(lv, "You have to select a protocol.");
            }
        }

        private void CreateInstanceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = (this.DialogResult == DialogResult.OK) && !this.ValidateChildren();
        }
    }
}