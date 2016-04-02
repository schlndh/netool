using Netool.Network.DataFormats;
using System;
using System.IO;
using System.Windows.Forms;

namespace Netool.Views.Editor
{
    public partial class FileEditor : Form, IEditorView
    {
        private string filename;

        public FileEditor()
        {
            InitializeComponent();
            MinimumSize = Size;
        }

        private void editBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = filenameLabel.Text = openFileDialog1.FileName;
            }
        }

        public string ID { get { return "File"; } }

        public void Clear()
        {
            filename = "";
            filenameLabel.Text = "Select file:";
        }

        public Network.DataFormats.IDataStream GetValue()
        {
            try
            {
                var stream = FromStream.ToIDataStream(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
                if (stream.Length > 5 * 1024 * 1024)
                {
                    return new LazyLoggedFile(stream);
                }
                else
                {
                    return new ByteArray(stream);
                }
            }
            catch
            {
                return EmptyData.Instance;
            }
        }

        public void SetValue(Network.DataFormats.IDataStream s)
        {
            MessageBox.Show("This action is not supported.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public Form GetForm()
        {
            return this;
        }
    }
}