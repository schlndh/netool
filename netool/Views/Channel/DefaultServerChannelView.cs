using Netool.Network;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Views.Channel
{
    public partial class DefaultServerChannelView : Form, IChannelView
    {
        public delegate void ColumnFiller(ListView.ColumnHeaderCollection c);
        public delegate ListViewItem ItemFactory(Netool.Event e);

        public static void DefaultColumnFiller(ListView.ColumnHeaderCollection c)
        {
            c.Add("ID");
            c.Add("Time").Width = -2;
            c.Add("Type").Width = -2;
        }

        public static ListViewItem DefaultItemFactory(Netool.Event e)
        {
            return new ListViewItem(new string[] { e.ID.ToString(), e.Time.ToString("HH:mm:ss.ff"), e.Type.ToString() });
        }

        private ChannelInfo info;
        private List<ListViewItem> cache = null;
        private int cacheStart = 0;
        private ItemFactory createItem;
        private IEventView currentViewForm = null;
        private Editor.EditorMasterView editor = null;

        public DefaultServerChannelView(ChannelInfo info)
            : this(info, DefaultColumnFiller, DefaultItemFactory)
        {
        }

        public DefaultServerChannelView(ChannelInfo info, ColumnFiller filler, ItemFactory factory)
        {
            InitializeComponent();
            this.info = info;
            this.events.VirtualListSize = info.GetEventCount();
            this.info.EventCountChanged += eventCountChanged;
            filler(this.events.Columns);
            createItem = factory;
            mainSplitContainer.Panel2Collapsed = true;
        }

        public void AllowManualControl(Editor.EditorMasterView editor)
        {
            mainSplitContainer.Panel2Collapsed = false;
            editor.TopLevel = false;
            editor.Visible = true;
            editor.FormBorderStyle = FormBorderStyle.None;
            editor.Dock = DockStyle.Fill;
            mainSplitContainer.Panel2.Controls.Add(editor);
            editor.SendClicked += editorSendHandler;
            editor.CloseClicked += editorCloseHandler;
            events.ContextMenuStrip = eventsContextMenu;
            this.editor = editor;
        }

        public void AddEventView(IEventView v)
        {
            eventViewsSelect.Items.Add(v);
            if (eventViewsSelect.SelectedIndex < 0) eventViewsSelect.SelectedIndex = 0;
        }

        public Form GetForm()
        {
            return this;
        }

        private void eventCountChanged(object sender, int e)
        {
            this.events.Invoke(new Action(() => this.events.VirtualListSize = e));
        }

        private void events_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (cache != null && e.ItemIndex >= cacheStart && e.ItemIndex < cacheStart + cache.Count)
            {
                e.Item = cache[e.ItemIndex - cacheStart];
            }
            else
            {
                e.Item = createItem(getEventByPosition(e.ItemIndex));
            }
        }

        private void events_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            // new cache is a subset of current cache
            if (cache != null && cacheStart <= e.StartIndex && cache.Count > e.EndIndex - e.StartIndex) return;
            cache = new List<ListViewItem>(e.EndIndex - e.StartIndex + 1);
            cacheStart = e.StartIndex;
            var node = info.GetByPosition(e.StartIndex);
            int i = 0;
            do
            {
                cache.Insert(i, createItem(node.Value));
                node = node.Next;
            } while (++i < e.EndIndex - e.StartIndex);
        }

        private void eventViewsSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (eventViewsSelect.SelectedIndex > -1)
            {
                eventViewPanel.Controls.Clear();
                currentViewForm = ((IEventView)eventViewsSelect.SelectedItem);
                var frm = currentViewForm.GetForm();
                frm.TopLevel = false;
                frm.Visible = true;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Dock = DockStyle.Fill;
                eventViewPanel.Controls.Add(frm);
                if(events.SelectedIndices.Count > 0)
                {
                    currentViewForm.Show(getEventByPosition(events.SelectedIndices[0]));
                }
            }
        }

        private void events_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (events.SelectedIndices.Count > 0)
            {
                if (currentViewForm != null)
                {
                    var evt = getEventByPosition(events.SelectedIndices[0]);
                    idLabel.Text = evt.ID.ToString();
                    typeLabel.Text = evt.Type.ToString();
                    timeLabel.Text = evt.Time.ToString("dd. MM. yyyy HH:mm:ss.ff");
                    currentViewForm.Show(evt);
                }
            }
            else
            {
                idLabel.Text = "-";
                typeLabel.Text = "-";
                timeLabel.Text = "-";
            }
        }

        private void editorSendHandler(object sender, IByteArrayConvertible val)
        {
            var channel = info.channel as IServerChannel;
            if (channel != null)
            {
                channel.Send(val);
            }
        }

        private void editorCloseHandler(object sender)
        {
            if (info.channel != null)
            {
                info.channel.Close();
            }
        }

        private void DefaultServerChannelView_FormClosed(object sender, FormClosedEventArgs e)
        {
            info.EventCountChanged -= eventCountChanged;
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if(item.Name == "eventsEditMenuItem")
            {
                if(editor != null && events.SelectedIndices.Count > 0)
                {
                    editor.SetValue(getEventByPosition(events.SelectedIndices[0]));
                }
            }
        }

        private Netool.Event getEventByPosition(int pos)
        {
            return info.GetByPosition(pos).Value;
        }
    }
}