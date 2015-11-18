using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Bridges.HexBox;
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
            data.ByteProvider = new DataStreamByteProvider(s);
        }

        public Form GetForm()
        {
            return this;
        }
    }
}