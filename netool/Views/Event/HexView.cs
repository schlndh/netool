using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(e.Data != null)
            {
                data.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(e.Data.Data.ToByteArray());
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
