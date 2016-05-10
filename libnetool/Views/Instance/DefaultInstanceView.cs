using Netool.Controllers;
using Netool.Logging;
using Netool.Network;
using Netool.Plugins.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Netool.Views.Instance
{
    /// <summary>
    /// Default implementation of IInstanceView interface
    /// </summary>
    /// <remarks>
    /// Don't forget to call <see cref="DefaultInstanceView.SetController" autoUpgrade="true">SetController</see>.
    /// </remarks>
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
            return new ListViewItem(new string[] { c.ID.ToString(), (c.Driver != null ? c.Driver.Type : "-"), c.Name });
        }

        private IInstanceController controller;
        private ItemFactory createItem;
        private StatusStrip instanceInfoStrip;

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

        /// <summary>
        /// Attaches view to controller and displays instance status strip
        /// </summary>
        /// <param name="c"></param>
        public void SetController(IInstanceController c)
        {
            controller = c;
            if(instanceInfoStrip == null)
            {
                instanceInfoStrip = InstanceStatusStripFactories.Factory(c.Instance.GetType(), c.Instance.Settings, c.Logger.IsTempFile, c.Logger.Filename);
                Controls.Add(instanceInfoStrip);
            }
        }

        /// <summary>
        /// Attaches view to controller with custom status strip
        /// </summary>
        /// <param name="c"></param>
        /// <param name="statusStrip">status strip to display or null</param>
        public void SetController(IInstanceController c, StatusStrip statusStrip)
        {
            controller = c;
            if(statusStrip != null)
            {
                instanceInfoStrip = statusStrip;
                Controls.Add(statusStrip);
            }
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
        }

        public Form GetForm()
        {
            return this;
        }

        private void stop_Click(object sender, EventArgs e)
        {
            Task.Run(() => controller.Stop());
            start.Enabled = true;
            stop.Enabled = false;
        }

        private void start_Click(object sender, EventArgs e)
        {
            Task.Run(() => controller.Start());
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
                    e.Item = createItem(logger.GetChannelByID(e.ItemIndex + 1));
                }
            }
        }

        private void channels_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if(logger != null)
            {
                // new cache is a subset of current cache
                if (cache != null && cacheStart <= e.StartIndex && cache.Count > e.EndIndex - e.StartIndex) return;
                cache = new List<ListViewItem>(logger.GetChannelRange(e.StartIndex + 1, e.EndIndex + 1 - e.StartIndex).Select(ch => createItem(ch)));
                cacheStart = e.StartIndex;
            }
        }

        /// <inheritdoc/>
        void IInstanceView.Close()
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.channels.VirtualListSize = logger.GetChannelCount();
        }
    }
}