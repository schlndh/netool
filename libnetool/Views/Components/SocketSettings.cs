using Netool.Network.Helpers;
using System.Windows.Forms;
using System.ComponentModel;
namespace Netool.Views.Components
{
    public partial class SocketSettings : UserControl
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SocketProperties Settings { get { return (SocketProperties)propertyGrid1.SelectedObject; } set { if(value != null) propertyGrid1.SelectedObject = value; } }

        public SocketSettings()
        {
            InitializeComponent();
            Settings = new SocketProperties();
        }
    }
}