using Netool.Dialogs;
using Netool.Network.DataFormats;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Netool.Views
{
    public partial class StreamWrapperView : BaseForm, IEventView, IEditorView
    {
        /// <inheritdoc />
        public string ID { get { return "StreamWrapper"; } }

        private bool isEditor;
        private IEnumerable<IStreamWrapperPlugin> wrapperPlugins;
        private IEnumerable<IStreamWrapper> usedWrappers = new List<IStreamWrapper>();

        public IEnumerable<IStreamWrapper> UsedWrappers { get { return usedWrappers; } set { usedWrappers = new List<IStreamWrapper>(value); refreshUsedWrappers(); } }

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
            this.MinimumSize = this.Size;
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
            this.MinimumSize = this.Size;
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
                tmp = w.Wrap(tmp);
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

        /// <summary>
        /// Sets or shows the stream depending on mode
        /// </summary>
        /// <param name="s"></param>
        public void SetValue(IDataStream s)
        {
            if (isEditor) ((IEditorView)this).SetValue(s);
            else ((IEventView)this).Show(s);
        }

        /// <summary>
        /// Shows stream and sets wrappers.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="newWrappers"></param>
        /// <remarks>
        /// Avoids setting wrappers to the old stream, which may be incompatible with new wrappers.
        /// </remarks>
        public void ShowWithNewWrappers(IDataStream s, IEnumerable<IStreamWrapper> newWrappers)
        {
            SetValue(null);
            UsedWrappers = newWrappers;
            SetValue(s);
        }

        private void wrapperEditBtn_Click(object sender, EventArgs e)
        {
            var dialog = new StreamWrapperViewSetupDialog(wrapperPlugins, usedWrappers);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                usedWrappers = dialog.UsedWrappers;
                refreshUsedWrappers();
            }
        }

        private void refreshUsedWrappers()
        {
            var formula = "DATA";
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

        private Size calculateMinSize()
        {
            var s = dataViewSelection.MinimumSize;
            s.Height += tableLayoutPanel2.Height + tableLayoutPanel1.Margin.Vertical + dataViewSelection.Margin.Vertical;
            return s;
        }

        private void dataViewSelection_MinimumSizeChanged(object sender, EventArgs e)
        {
            this.MinimumSize = calculateMinSize();
        }
    }
}