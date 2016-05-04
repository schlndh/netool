using Netool.Network.DataFormats;
using System.Text;
using System.Windows.Forms;

namespace Netool.Views.Editor
{
    public partial class Utf8TextEditor : Form, IEditorView
    {
        /// <inheritdoc/>
        public string ID { get { return "UTF-8 text"; } }

        public Utf8TextEditor()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            textBox1.Text = "";
        }

        /// <inheritdoc/>
        public Form GetForm()
        {
            return this;
        }

        /// <inheritdoc/>
        public IDataStream GetValue()
        {
            return new ByteArray(textBox1.Text, UTF8Encoding.UTF8);
        }

        /// <inheritdoc/>
        public void SetValue(IDataStream s)
        {
            textBox1.Text = UTF8Encoding.UTF8.GetString(s.ReadBytes());
        }
    }
}