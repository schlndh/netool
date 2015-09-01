using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Netool.Plugins.ChannelDrivers.Dialogs
{
    public partial class ManualChannelDriverSettingsDialog : Form
    {
        public int Capacity { get { return (int)capacityLimit.Value; } }

        public ManualChannelDriverSettingsDialog()
        {
            InitializeComponent();
        }
    }
}
