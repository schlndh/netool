using Netool.Network.DataFormats;
using System.Text;
using System.Windows.Forms;

namespace Netool.Views.Event
{
    public partial class Utf8TextView : Form, IEventView
    {
        /// <inheritdoc/>
        public string ID { get { return "UTF-8 text"; } }

        public Utf8TextView()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public Form GetForm()
        {
            return this;
        }

        /// <inheritdoc/>
        public void Show(IDataStream s)
        {
            textBox1.Text = UTF8Encoding.UTF8.GetString(s.ReadBytes());
        }
    }
}