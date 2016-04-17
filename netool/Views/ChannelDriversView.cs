using Netool.Controllers;
using Netool.Plugins;
using System;
using System.Windows.Forms;

namespace Netool.Views
{
    public partial class ChannelDriversView : Form
    {
        private MainController controller;

        public ChannelDriversView()
        {
            InitializeComponent();
        }

        public void SetController(MainController c)
        {
            controller = c;
        }

        public void AddChannelDriver(int id, ChannelDriverPack pack)
        {
            var item = new ListViewItem(new string[] { pack.Driver.Name, pack.Driver.Type });
            item.Tag = new Tuple<int, ChannelDriverPack>(id, pack);
            this.channelDrivers.Items.Add(item);
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.channelDrivers.SelectedItems)
            {
                this.channelDrivers.Items.Remove(item);
                controller.RemoveChannelDriver(((Tuple<int, ChannelDriverPack>)item.Tag).Item1);
            }
        }

        private void detailBtn_Click(object sender, EventArgs e)
        {
            if (this.channelDrivers.SelectedItems.Count > 0)
            {
                var pack = (Tuple<int, ChannelDriverPack>)channelDrivers.SelectedItems[0].Tag;
                if (pack.Item2.View != null)
                {
                    pack.Item2.View.GetForm().Show();
                }
            }
        }
    }
}