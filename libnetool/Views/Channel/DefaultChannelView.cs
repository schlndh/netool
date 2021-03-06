﻿using Netool.Dialogs;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Helpers;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Netool.Views.Channel
{
    public partial class DefaultChannelView : Form, IChannelView
    {
        public delegate void ColumnFiller(ListView.ColumnHeaderCollection c);
        public delegate ListViewItem ItemFactory(Netool.Logging.Event e);

        public static void DefaultColumnFiller(ListView.ColumnHeaderCollection c)
        {
            c.Add("ID");
            c.Add("Time").Width = -2;
            c.Add("Type").Width = -2;
            c.Add("Data").Width = -2;
        }

        public static ListViewItem DefaultItemFactory(Netool.Logging.Event e)
        {
            string type = e.Type.ToString();
            string data = "";
            if (e.Type == EventType.ChannelReplaced)
            {
                data = e.Channel.GetType().Name;
            }
            else if(e.Data != null && e.Data.Data != null)
            {
                data = e.Data.Data.ToString();
            }
            return new ListViewItem(new string[] { e.ID.ToString(), e.Time.ToString("HH:mm:ss.ff"), type, data });
        }

        private ChannelLogger logger;
        private List<ListViewItem> cache = null;
        private int cacheStart = 0;
        private ItemFactory createItem;
        private Editor.EditorMasterView editor = null;
        private IChannelExtensions.ChannelHandlers handlers;
        private List<IMessageTemplate> messageTemplates = new List<IMessageTemplate>();
        private AsyncActionQueue actionQueue = new AsyncActionQueue();

        public IChannel Channel { get { return logger.channel; } }

        public DefaultChannelView(ChannelLogger logger)
            : this(logger, DefaultColumnFiller, DefaultItemFactory)
        {
        }

        public DefaultChannelView(ChannelLogger logger, ColumnFiller filler, ItemFactory factory)
        {
            InitializeComponent();
            channelMenu.Visible = channelMenu.Items.Count > 0;
            this.logger = logger;
            this.events.VirtualListSize = logger.GetEventCount();
            filler(this.events.Columns);
            createItem = factory;
            var r = logger.channel as IReplaceableChannel;
            handlers = new IChannelExtensions.ChannelHandlers { ChannelReplaced = channelReplacedHandler };
            templatesToolStripMenuItem.Visible = false;
            // hide by default
            viewsTabControl.TabPages.Remove(editorTabPage);
        }

        private void channelReplacedHandler(object sender, IChannel e)
        {
            if(editor != null)
            {
                editor.SetProxy(e is IProxyChannel);
            }
            ((IChannel)sender).UnbindAllEvents(handlers);
            e.BindAllEvents(handlers);
        }

        /// <summary>
        /// Allow user to manually control the channel.
        /// </summary>
        /// <param name="editor">editor to display</param>
        /// <remarks>
        /// Note: control will be shared with channel's driver.
        /// </remarks>
        public void AllowManualControl(Editor.EditorMasterView editor)
        {
            editorTabPage.Embed(editor);
            editor.SendClicked += editorSendHandler;
            editor.CloseClicked += editorCloseHandler;
            events.ContextMenuStrip = eventsContextMenu;
            this.editor = editor;
            if(logger.channel != null && logger.channel is IProxyChannel)
            {
                this.editor.SetProxy(true);
            }
            templatesToolStripMenuItem.Visible = messageTemplates.Count > 0;
            viewsTabControl.TabPages.Add(editorTabPage);
        }

        public void AddMessageTemplates(IEnumerable<IMessageTemplatePlugin> templatePlugins)
        {
            foreach(var pl in templatePlugins)
            {
                messageTemplates.AddRange(pl.CreateTemplates());
            }
            templatesToolStripMenuItem.Visible = editor != null && messageTemplates.Count > 0;
        }

        public void AddEventViews(IEnumerable<IEventViewPlugin> viewPlugins, Type defaultView = null)
        {
            dataView.AddEventViews(viewPlugins, defaultView ?? typeof(Event.HexView));
        }

        public Form GetForm()
        {
            return this;
        }

        public void AddMenuItem(ToolStripMenuItem item)
        {
            channelMenu.Items.Add(item);
            channelMenu.Visible = true;
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
            if (cache != null && cacheStart <= e.StartIndex && cacheStart + cache.Count > e.EndIndex) return;
            cache = new List<ListViewItem>(logger.GetEventRange(e.StartIndex + 1, e.EndIndex + 1 - e.StartIndex).Select(ev => createItem(ev)));
            cacheStart = e.StartIndex;
        }

        private void events_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (events.SelectedIndices.Count > 0)
            {
                var evt = getEventByPosition(events.SelectedIndices[0]);
                showEvent(evt);
            }
            else
            {
                idLabel.Text = "-";
                typeLabel.Text = "-";
                timeLabel.Text = "-";
            }
        }

        private void showEvent(Logging.Event e)
        {
            idLabel.Text = e.ID.ToString();
            typeLabel.Text = e.Type.ToString();
            timeLabel.Text = e.Time.ToString("dd. MM. yyyy HH:mm:ss.ff");
            if (e.Data != null && e.Data.Data != null)
            {
                try
                {
                    dataView.Stream = e.Data.Data;
                }
                catch (UnsupportedDataStreamException)
                {
                    MessageBox.Show("Given data stream is not supported by current view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                dataView.Stream = null;
            }

        }

        private void editorSendHandler(object sender, Editor.EditorMasterView.SendEventArgs e)
        {
            actionQueue.Add(
                delegate ()
                {
                    IChannel channel = logger.channel;
                    if (channel != null)
                    {
                        if (channel is IClientChannel)
                        {
                            ((IClientChannel)channel).Send(e.Data);
                        }
                        else if (channel is IServerChannel)
                        {
                            ((IServerChannel)channel).Send(e.Data);
                        }
                        else if (channel is IProxyChannel)
                        {
                            if (e.ToClient)
                            {
                                ((IProxyChannel)channel).SendToClient(e.Data);
                            }
                            else
                            {
                                ((IProxyChannel)channel).SendToServer(e.Data);
                            }
                        }
                    }
                });
        }

        private void editorCloseHandler(object sender)
        {
            actionQueue.Add(
                delegate ()
                {
                    if (logger.channel != null)
                    {
                            logger.channel.Close();
                    }
                });
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if(item.Name == "eventsEditMenuItem")
            {
                if(editor != null && events.SelectedIndices.Count > 0)
                {
                    var evt = getEventByPosition(events.SelectedIndices[0]);
                    if(evt.Data != null && evt.Data.Data != null)
                    {
                        viewsTabControl.SelectedTab = editorTabPage;
                        editor.SetValue(evt.Data.Data);
                    }
                    else
                    {
                        editor.Clear();
                    }
                }
            }
        }

        private Logging.Event getEventByPosition(int pos)
        {
            // position is 0-indexed, whereas id is 1-indexed
            return logger.GetEventByID(pos + 1);
        }

        private void templatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new MessageTemplateSelectionDialog(messageTemplates);
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                var s = dialog.DataStream;
                editor.SetValue(s);
            }
        }

        private void dataView_MinimumSizeChanged(object sender, EventArgs e)
        {
            var s = dataView.MinimumSize;
            s.Height += flowLayoutPanel1.Height + flowLayoutPanel1.Margin.Vertical + dataView.Margin.Vertical;
            eventScrollPanel.AutoScrollMinSize = s;
            dataView.Invalidate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.events.VirtualListSize = logger.GetEventCount();
        }

        private void DefaultChannelView_FormClosing(object sender, FormClosingEventArgs e)
        {
            actionQueue.Stop();
        }
    }
}