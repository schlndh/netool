using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Netool.Dialogs.StreamWrappers
{
    public partial class StreamSegmentSetupDialog : Form
    {
        public long Offset { get; private set; }
        public long Count { get; private set; }

        public StreamSegmentSetupDialog()
        {
            InitializeComponent();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Offset = (beginRadio.Checked ? 1 : -1) * long.Parse(offsetTextBox.Text);
            Count = (exactRadio.Checked ? long.Parse(countTextBox.Text) : -1);
        }

        private void toEndRadio_CheckedChanged(object sender, EventArgs e)
        {
            countTextBox.Enabled = false;
            countTextBox.CausesValidation = false;
        }

        private void exactRadio_CheckedChanged(object sender, EventArgs e)
        {
            countTextBox.Enabled = true;
            countTextBox.CausesValidation = true;
        }

        private void validateLong(object sender, CancelEventArgs e)
        {
            long l;
            e.Cancel = !long.TryParse((sender as TextBox).Text, out l) || l < 0;
        }
    }
}
