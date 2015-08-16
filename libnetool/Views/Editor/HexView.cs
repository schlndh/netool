using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Netool.Network;
using Netool.Network.DataFormats;
namespace Netool.Views.Editor
{
    public partial class HexView : Form, IEditorView
    {
        public string ID { get { return "HexView"; } }

        public HexView()
        {
            InitializeComponent();
            data.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(new byte[]{});
        }

        public void Clear()
        {
            data.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(new byte[] { });
        }

        public Form GetForm()
        {
            return this;
        }

        public IDataStream GetValue()
        {
            return new ByteArray(((Be.Windows.Forms.DynamicByteProvider)data.ByteProvider).Bytes.ToArray());
        }

        public void SetValue(IDataStream s)
        {
            // TODO: improve this
            data.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(s.ReadBytes());
        }
    }
}
