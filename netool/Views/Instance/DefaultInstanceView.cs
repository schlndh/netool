using Netool.Controllers;
using Netool.Logging;
using Netool.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Netool.Views.Instance
{
    public partial class DefaultInstanceView : Form, IInstanceView
    {
        public delegate void ColumnFiller(ListView.ColumnHeaderCollection c);
        public delegate ListViewItem ItemFactory(IChannel c);
        private List<ListViewItem> cache = null;
        private int cacheStart = 0;
        private InstanceLogger logger;

        public static void DefaultColumnFiller(ListView.ColumnHeaderCollection c)
        {
            c.Add("ID");
            c.Add("Driver").Width = -2;
            c.Add("Name").Width = -2;
        }

        public static ListViewItem DefaultItemFactory(IChannel c)
        {
            return new ListViewItem(new string[] { c.ID.ToString(), (c.Driver != null ? c.Driver.ID : "-"), c.Name });
        }

        private IInstanceController controller;
        private ItemFactory createItem;

        /// <summary>
        /// Default view settings with default DataTable schema and corresponding row factory
        /// </summary>
        public DefaultInstanceView()
            : this(DefaultInstanceView.DefaultColumnFiller, DefaultInstanceView.DefaultItemFactory)
        { }

        public DefaultInstanceView(ColumnFiller filler, ItemFactory rowFactory)
        {
            InitializeComponent();
            filler(this.channels.Columns);
            this.createItem = rowFactory;
        }

        public void SetController(IInstanceController c)
        {
            controller = c;
        }

        public void SetInstance(IInstance s)
        {
            start.Enabled = !s.IsStarted;
            stop.Enabled = !start.Enabled;
        }

        public void SetLogger(InstanceLogger logger)
        {
            this.logger = logger;
            this.channels.VirtualListSize = logger.GetChannelCount();
            logger.ChannelCountChanged += logger_ChannelCountChanged;
        }

        private void logger_ChannelCountChanged(object sender, int e)
        {
            this.channels.Invoke(new Action(() => this.channels.VirtualListSize = e));
        }

        public Form GetForm()
        {
            return this;
        }

        private void stop_Click(object sender, EventArgs e)
        {
            controller.Stop();
            start.Enabled = true;
            stop.Enabled = false;
        }

        private void start_Click(object sender, EventArgs e)
        {
            controller.Start();
            start.Enabled = false;
            stop.Enabled = true;
        }

        private void channels_DoubleClick(object sender, EventArgs e)
        {
            if(channels.SelectedIndices.Count > 0)
            {
                controller.ShowDetail(channels.SelectedIndices[0]+1);
            }
        }

        private void channels_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if(logger != null)
            {

                if (cache != null && e.ItemIndex >= cacheStart && e.ItemIndex < cacheStart + cache.Count)
                {
                    e.Item = cache[e.ItemIndex - cacheStart];
                }
                else
                {
                    // ID is 1-based
                    e.Item = createItem(logger.GetChannelByID(e.ItemIndex + 1).Value);
                }
            }
        }

        private void channels_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if(logger != null)
            {
                // new cache is a subset of current cache
                if (cache != null && cacheStart <= e.StartIndex && cache.Count > e.EndIndex - e.StartIndex) return;
                cache = new List<ListViewItem>(e.EndIndex - e.StartIndex + 1);
                cacheStart = e.StartIndex;
                // ID is 1-based
                var node = logger.GetChannelByID(e.StartIndex + 1);
                int i = 0;
                do
                {
                    cache.Insert(i, createItem(node.Value));
                    node = node.Next;
                } while (++i < e.EndIndex - e.StartIndex);
            }
        }
    }
}