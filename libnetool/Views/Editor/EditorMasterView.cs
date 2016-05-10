using Netool.Views.Components;
using Netool.Network.DataFormats;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Views.Editor
{
    public partial class EditorMasterView : Form
    {
        public struct SendEventArgs
        {
            public IDataStream Data;
            /// <summary>
            /// Indicates whether Data is to be sent to client or server, only matters for proxy
            /// </summary>
            public bool ToClient;
        }
        public delegate void CloseClickedHandler(object sender);
        public event EventHandler<SendEventArgs> SendClicked;
        public event CloseClickedHandler CloseClicked;
        private float origProxyRowHeight;

        public EditorMasterView(IEnumerable<IEditorViewPlugin> editorPlugins, Type defaultEditor = null)
        {
            if (editorPlugins == null) throw new ArgumentNullException("editorPlugins");
            if (defaultEditor == null) defaultEditor = typeof(Editor.HexView);
            InitializeComponent();
            this.innerEditors.AddEditors(editorPlugins, defaultEditor);
            origProxyRowHeight = this.tableLayoutPanel1.RowStyles[0].Height;
            SetProxy(false);
        }

        private void InnerEditors_MinimumSizeChanged(object sender, EventArgs e)
        {
            this.AutoScrollMinSize = calculateMinSize();
        }

        public void SetValue(IDataStream s)
        {
            try
            {
                innerEditors.Stream = s;
            }
            catch (UnsupportedDataStreamException)
            {
                innerEditors.Stream = null;
                MessageBox.Show("Given data stream is not supported by current view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear()
        {
            innerEditors.Stream = null;
        }

        /// <summary>
        /// Enables/disables special proxy controls
        /// </summary>
        /// <param name="proxy">is current channel proxy?</param>
        public void SetProxy(bool proxy)
        {
            proxyFlowPanel.Visible = proxy;
            tableLayoutPanel1.RowStyles[0].Height = (proxy) ? origProxyRowHeight : 0F;
        }

        private System.Drawing.Size calculateMinSize()
        {
            var s = innerEditors.MinimumSize;
            s.Height += flowLayoutPanel1.Height + flowLayoutPanel1.Margin.Vertical
                + proxyFlowPanel.Height + proxyFlowPanel.Margin.Vertical
                + innerEditors.Margin.Vertical;
            return s;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            IDataStream val;
            try
            {
                val = innerEditors.Stream;
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Validation failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var ev = SendClicked;
            if (ev != null && val != null)
            {
                ev(this, new SendEventArgs { Data = val, ToClient = proxyRadioClient.Checked});
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            var ev = CloseClicked;
            if (ev != null)
            {
                ev(this);
            }
        }
    }
}