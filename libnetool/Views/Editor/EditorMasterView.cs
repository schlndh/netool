﻿using Netool.Network;
using Netool.Network.DataFormats;
using System;
using System.Windows.Forms;

namespace Netool.Views.Editor
{
    public partial class EditorMasterView : Form
    {
        public struct SendEventArgs
        {
            public IInMemoryData Data;
            /// <summary>
            /// Indicates whether Data is to be sent to client or server, only matters for proxy
            /// </summary>
            public bool ToClient;
        }
        public delegate void CloseClickedHandler(object sender);
        public event EventHandler<SendEventArgs> SendClicked;
        public event CloseClickedHandler CloseClicked;

        public EditorMasterView()
        {
            InitializeComponent();
            proxyFlowPanel.Visible = false;
        }

        public void AddEditor(IEditorView v)
        {
            editorViewSelect.Items.Add(v);
            if (editorViewSelect.SelectedIndex < 0) editorViewSelect.SelectedIndex = 0;
        }

        public void SetValue(Netool.Logging.Event val)
        {
            if(editorViewSelect.SelectedIndex > -1)
            {
                ((IEditorView)editorViewSelect.SelectedItem).SetValue(val);
            }
        }

        /// <summary>
        /// Enables/disables special proxy controls
        /// </summary>
        /// <param name="proxy">is current channel proxy?</param>
        public void SetProxy(bool proxy)
        {
            proxyFlowPanel.Visible = proxy;
        }

        private void editorViewSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (editorViewSelect.SelectedIndex > -1)
            {
                editorViewPanel.Controls.Clear();
                var frm = ((IEditorView)editorViewSelect.SelectedItem).GetForm();
                frm.TopLevel = false;
                frm.Visible = true;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Dock = DockStyle.Fill;
                editorViewPanel.Controls.Add(frm);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            if (editorViewSelect.SelectedIndex > -1)
            {
                ((IEditorView)editorViewSelect.SelectedItem).Clear();
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (editorViewSelect.SelectedIndex > -1)
            {
                var val = ((IEditorView)editorViewSelect.SelectedItem).GetValue();
                if (SendClicked != null)
                {
                    SendClicked(this, new SendEventArgs { Data = val, ToClient = proxyRadioClient.Checked});
                }
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (CloseClicked != null)
            {
                CloseClicked(this);
            }
        }
    }
}