using Netool.Dialogs;
using Netool.Network.DataFormats;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Views
{
    public partial class StreamWrapperView : Form, IEventView, IEditorView
    {
        private bool isEditor;
        private IEnumerable<IStreamWrapperPlugin> wrapperPlugins;
        private IEnumerable<IStreamWrapper> usedWrappers = new List<IStreamWrapper>();

        /// <summary>
        /// current stream for EventView
        /// </summary>
        private IDataStream currentStream;

        /// <summary>
        /// Constructor for IEventView version
        /// </summary>
        /// <param name="wrapperPlugins"></param>
        /// <param name="innerViews"></param>
        public StreamWrapperView(IEnumerable<IStreamWrapperPlugin> wrapperPlugins, IEnumerable<IEventView> innerViews)
        {
            InitializeComponent();
            this.wrapperPlugins = wrapperPlugins;
            this.isEditor = dataViewSelection.IsEditor = false;
            foreach (var v in innerViews)
            {
                dataViewSelection.InnerViews.Add(v);
                if (v is Event.HexView)
                {
                    dataViewSelection.SelectedIndex = dataViewSelection.InnerViews.Count - 1;
                }
            }
        }

        /// <summary>
        /// Constructor for IEditorView version
        /// </summary>
        /// <param name="wrapperPlugins"></param>
        /// <param name="innerEditors"></param>
        public StreamWrapperView(IEnumerable<IStreamWrapperPlugin> wrapperPlugins, IEnumerable<IEditorView> innerEditors)
        {
            InitializeComponent();
            this.wrapperPlugins = wrapperPlugins;
            this.isEditor = dataViewSelection.IsEditor = true;
            foreach (var v in innerEditors)
            {
                dataViewSelection.InnerEditors.Add(v);
                if (v is Event.HexView)
                {
                    dataViewSelection.SelectedIndex = dataViewSelection.InnerEditors.Count - 1;
                }
            }
        }

        /// <inheritdoc />
        public string ID { get { return "StreamWrapper"; } }

        /// <inheritdoc />
        void IEventView.Show(IDataStream s)
        {
            dataViewSelection.Stream = applyWrappers(s);
            currentStream = s;
        }

        private IDataStream applyWrappers(IDataStream s)
        {
            if (s == null) return null;
            var tmp = s;
            foreach (var w in usedWrappers)
            {
                tmp = w.Wrap(s);
            }
            return tmp;
        }

        /// <inheritdoc />
        public Form GetForm()
        {
            return this;
        }

        /// <inheritdoc />
        void IEditorView.Clear()
        {
            dataViewSelection.Stream = null;
        }

        /// <inheritdoc />
        IDataStream IEditorView.GetValue()
        {
            return applyWrappers(dataViewSelection.Stream);
        }

        /// <inheritdoc />
        void IEditorView.SetValue(IDataStream s)
        {
            dataViewSelection.Stream = s;
        }

        private void wrapperEditBtn_Click(object sender, EventArgs e)
        {
            var dialog = new StreamWrapperViewSetupDialog(wrapperPlugins, usedWrappers);
            var formula = "DATA";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                usedWrappers = dialog.UsedWrappers;
                foreach (var w in usedWrappers)
                {
                    if (string.IsNullOrEmpty(w.Params))
                    {
                        formula = w.Name + "(" + formula + ")";
                    }
                    else
                    {
                        formula = w.Name + "(" + formula + ", " + w.Params + ")";
                    }
                }
                selectedWrapper.Text = formula;
                if (!isEditor)
                {
                    dataViewSelection.Stream = applyWrappers(currentStream);
                }
            }
        }
    }
}