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

        public void Show(Netool.Logging.Event e)
        {
            if (e.Data != null)
            {
                // TODO: improve this
                data.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(e.Data.Data.ReadBytes(0, e.Data.Data.Length));
                data.Visible = true;
            }
            else
            {
                data.ByteProvider = null;
                data.Visible = false;
            }
        }

        public Form GetForm()
        {
            return this;
        }
    }
}