using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class TextBoxDialog : Form
    {
        /// <summary>
        /// Gets value entered by user - only valid if DialogResult is OK
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// This event occurs when the TextBox is being validated, sender will be the TextBox not TextBoxDialog
        /// </summary>
        public event EventHandler<CancelEventArgs> ValidatingValue;

        public string DialogTitle { get { return this.Text; } set { this.Text = value; } }

        public string DialogText { get { return dialogLabel.Text; } set { dialogLabel.Text = value; } }

        public TextBoxDialog()
        {
            InitializeComponent();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Value = valueTextBox.Text;
        }

        private void valueTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (ValidatingValue != null) ValidatingValue(sender, e);
        }
    }
}