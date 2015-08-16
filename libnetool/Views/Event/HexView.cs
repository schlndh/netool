using Netool.Network.DataFormats;
using System.Windows.Forms;

namespace Netool.Views.Event
{
    public partial class HexView : Form, IEventView
    {
        public string ID { get { return "HexView"; } }

        public HexView()
        {
            InitializeComponent();
        }

        public void Show(IDataStream s)
        {
            // TODO: improve this
            data.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(s.ReadBytes());
        }

        public Form GetForm()
        {
            return this;
        }
    }
}