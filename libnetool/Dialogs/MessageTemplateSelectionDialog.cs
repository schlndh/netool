using Netool.Network.DataFormats;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class MessageTemplateSelectionDialog : Form
    {
        /// <summary>
        /// Gets DataStream created by the selected template or null
        /// </summary>
        public IDataStream DataStream { get; private set; }

        public MessageTemplateSelectionDialog(IEnumerable<IMessageTemplate> templates)
        {
            InitializeComponent();
            foreach (var t in templates)
            {
                templatesListBox.Items.Add(t);
                templatesListBox.DisplayMember = "Name";
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (templatesListBox.SelectedItem == null) DataStream = null;
            else
            {
                DataStream = ((IMessageTemplate)templatesListBox.SelectedItem).CreateMessage();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DataStream = null;
        }
    }
}