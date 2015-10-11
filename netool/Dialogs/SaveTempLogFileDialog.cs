using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class SaveTempLogFileDialog : Form
    {
        /// <summary>
        /// Indicates wheter "To all" button variant was clicked
        /// </summary>
        public bool ToAll { get; private set; }

        public SaveTempLogFileDialog(string instanceName)
        {
            InitializeComponent();
            iconPictureBox.Image = System.Drawing.SystemIcons.Question.ToBitmap();
            messageLabel.Text = "Instance log for " + instanceName + " is a temporary file. Do you wish to save it?";
        }

        private void yesToAllBtn_Click(object sender, System.EventArgs e)
        {
            ToAll = true;
        }

        private void noToAllBtn_Click(object sender, System.EventArgs e)
        {
            ToAll = true;
        }

        private void yesBtn_Click(object sender, System.EventArgs e)
        {
            ToAll = false;
        }

        private void noBtn_Click(object sender, System.EventArgs e)
        {
            ToAll = false;
        }
    }
}