using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Netool.ChannelDrivers;

namespace Netool.Dialogs
{
    public partial class SetupChannelDriversDialog : Form
    {
        public Dictionary<int, int> Drivers
        {
            get
            {
                var dict = new Dictionary<int, int>();
                int order = 0;
                foreach(int driver in orderedDrivers)
                {
                    dict[driver] = order++;
                }
                return dict;
            }
        }
        private List<int> orderedDrivers = new List<int>();
        private Dictionary<int, IChannelDriver> availableDrivers;

        public SetupChannelDriversDialog(Dictionary<int, IChannelDriver> availableDrivers)
        {
            InitializeComponent();
            this.availableDrivers = availableDrivers;
            foreach(var item in availableDrivers)
            {
                this.channelDriverSelect.Items.Add(new Tuple<int, string, string>(item.Key, item.Value.Name, item.Value.Type));
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            if(this.channelDriverSelect.SelectedIndex > -1)
            {
                var item = channelDriverSelect.SelectedItem as Tuple<int, string, string>;

                if(!orderedDrivers.Contains(item.Item1))
                {
                    orderedDrivers.Add(item.Item1);
                    var i = new ListViewItem(new string[] { item.Item2, item.Item3 });
                    i.Tag = item;
                    channelDrivers.Items.Add(i);
                }
                channelDriverSelect.Items.Remove(channelDriverSelect.SelectedItem);
            }
        }

        private void channelDrivers_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                removeSelectedDrivers();
            }
        }

        private void removeSelectedDrivers()
        {
            foreach (ListViewItem item in channelDrivers.SelectedItems)
            {
                var tuple = item.Tag as Tuple<int, string, string>;
                channelDriverSelect.Items.Add(tuple);
                Drivers.Remove((int)tuple.Item1);
                channelDrivers.Items.Remove(item);
            }
        }
    }
}
