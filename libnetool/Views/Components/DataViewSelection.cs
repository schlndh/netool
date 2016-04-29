using Netool.Network.DataFormats;
using Netool.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace Netool.Views.Components
{
    public partial class DataViewSelection : UserControl
    {

        private class TypedIListAdapter<T> : IList<T>
        {
            private IList inner;

            public TypedIListAdapter(IList innerList)
            {
                inner = innerList;
            }

            public int IndexOf(T item)
            {
                return inner.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                inner.Insert(index, item);
            }

            public T this[int index]
            {
                get
                {
                    return (T)inner[index];
                }
                set
                {
                    inner[index] = value;
                }
            }

            public void Add(T item)
            {
                inner.Add(item);
            }

            public bool Contains(T item)
            {
                return inner.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                inner.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                if (inner.Contains(item))
                {
                    inner.Remove(item);
                    return true;
                }
                return false;
            }

            public IEnumerator<T> GetEnumerator()
            {
                var e = inner.GetEnumerator();
                while (e.MoveNext())
                {
                    yield return (T)e.Current;
                }
            }

            public void RemoveAt(int index)
            {
                inner.RemoveAt(index);
            }

            public void Clear()
            {
                inner.Clear();
            }

            public int Count
            {
                get { return inner.Count; }
            }

            public bool IsReadOnly
            {
                get { return inner.IsReadOnly; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return inner.GetEnumerator();
            }
        }

        private Size minSize;
        public override Size MinimumSize
        {
            get
            {
                return minSize;
            }

            set
            {
                var oldMinSize = minSize;
                minSize = value;
                if (MinimumSizeChanged != null && oldMinSize != minSize) MinimumSizeChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler MinimumSizeChanged;

        private class ExportArgs
        {
            public IDataStream Input;
            public string Filename;
        }

        private IDataStream stream;
        private IEventView currentView;
        private IEditorView currentEditor;

        private bool isEditor = false;

        /// <summary>
        /// Gets or sets whether this component should behave as an editor.
        /// </summary>
        /// <remarks>
        /// If you change the value you have to add inner views/editors again!
        /// </remarks>
        public bool IsEditor
        {
            get { return isEditor; }
            set
            {
                if (isEditor != value)
                {
                    innerViewSelect.Items.Clear();
                }
                isEditor = value;
            }
        }

        /// <summary>
        /// Gets or sets stream to display. Set it to null if there is no data to be displayed.
        /// </summary>
        /// <remarks>
        /// Getter calls GetValue() if IsEditor is true.
        /// Setter calls Clear() if IsEditor and value == null.
        /// </remarks>
        public IDataStream Stream
        {
            get { return !isEditor ? stream : (currentEditor != null ? currentEditor.GetValue() : null); }
            set
            {
                if(isEditor)
                {
                    if (currentEditor == null) return;
                    if (value == null) currentEditor.Clear();
                    else currentEditor.SetValue(value);
                }
                else
                {
                    stream = value;
                    showStream();
                }
            }
        }

        public string Label { get { return innerViewSelectLabel.Text; } set { innerViewSelectLabel.Text = value; } }
        private TypedIListAdapter<IEventView> innerViews;
        private TypedIListAdapter<IEditorView> innerEditors;
        private Form innerForm;

        /// <summary>
        /// Get a list of possible inner views or null if IsEditor
        /// </summary>
        public IList<IEventView> InnerViews { get { return isEditor ? null : innerViews; } }

        /// <summary>
        /// Get a list of possible inner editors or null if !IsEditor
        /// </summary>
        public IList<IEditorView> InnerEditors { get { return !isEditor ? null : innerEditors; } }

        /// <summary>
        /// Gets or sets the index of the selected view/editor.
        /// </summary>
        public int SelectedIndex { get { return innerViewSelect.SelectedIndex; } set { innerViewSelect.SelectedIndex = value; } }

        /// <summary>
        /// Gets or sets the selected view/editor.
        /// </summary>
        public object SelectedItem { get { return innerViewSelect.SelectedItem; } set { innerViewSelect.SelectedItem = value; } }

        public DataViewSelection()
        {
            InitializeComponent();
            innerViews = new TypedIListAdapter<IEventView>(innerViewSelect.Items);
            innerEditors = new TypedIListAdapter<IEditorView>(innerViewSelect.Items);
        }

        /// <summary>
        /// Add all editors at once
        /// </summary>
        /// <param name="editors"></param>
        /// <param name="defaultEditor">should not embed other editors (infinite embedding)!</param>
        public void AddEditors(IEnumerable<IEditorViewPlugin> editors, Type defaultEditor = null)
        {
            if (!isEditor) throw new InvalidOperationException("Adding editors to non-editor DataViewSelection");
            if (defaultEditor == null) defaultEditor = typeof(Editor.HexView);
            foreach (var pl in editors)
            {
                foreach (var v in pl.CreateEditorViews())
                {
                    InnerEditors.Add(v);
                    if (v.GetType() == defaultEditor)
                    {
                        SelectedIndex = InnerEditors.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Add all editors at once
        /// </summary>
        /// <param name="eventViews"></param>
        /// <param name="defaultEventView">should not embed other editors (infinite embedding)!</param>
        public void AddEventViews(IEnumerable<IEventViewPlugin> eventViews, Type defaultEventView = null)
        {
            if (isEditor) throw new InvalidOperationException("Adding event views to editor DataViewSelection");
            if (defaultEventView == null) defaultEventView = typeof(Event.HexView);
            foreach (var pl in eventViews)
            {
                foreach (var v in pl.CreateEventViews())
                {
                    InnerViews.Add(v);
                    if (v.GetType() == defaultEventView)
                    {
                        SelectedIndex = InnerViews.Count - 1;
                    }
                }
            }
        }

        private void exportBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (exportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                btn.Text = "Cancel";
                btn.Click -= exportBtn_Click;
                btn.Click += cancelBtn_Click;
                var args = new ExportArgs { Input = Stream, Filename = exportSaveFileDialog.FileName };
                if(args.Input != null) exportBackgroundWorker.RunWorkerAsync(args);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            exportBackgroundWorker.CancelAsync();
            var btn = (Button)sender;
            btn.Text = "Export to file";
            btn.Click -= cancelBtn_Click;
            btn.Click += exportBtn_Click;
        }

        private void innerViewSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (innerViewSelect.SelectedIndex > -1)
            {
                if(!isEditor)
                {
                    currentView = ((IEventView)innerViewSelect.SelectedItem);
                }
                else
                {
                    currentEditor = ((IEditorView)innerViewSelect.SelectedItem);
                }
            }
            else
            {
                currentView = null;
                currentEditor = null;
            }
            try
            {
                showStream();
            }
            catch(UnsupportedDataStreamException)
            {
                MessageBox.Show("Given data stream is not supported by current view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void showStream()
        {
            innerViewPanel.Controls.Clear();
            if (innerForm != null) innerForm.MinimumSizeChanged -= Form_MinimumSizeChanged;
            if (!isEditor)
            {
                exportBtn.Enabled = stream != null;
                if (currentView == null || stream == null) return;
                innerForm = currentView.GetForm();
            }
            else
            {
                if (currentEditor == null) return;
                exportBtn.Enabled = true;
                innerForm = currentEditor.GetForm();
            }
            if (innerForm.MinimumSize.Height == 0) innerForm.MinimumSize = innerForm.Size;
            innerViewPanel.Embed(innerForm);
            Form_MinimumSizeChanged(innerForm, EventArgs.Empty);
            innerForm.MinimumSizeChanged += Form_MinimumSizeChanged;
            if(!isEditor)
            {
                currentView.Show(stream);
            }
        }

        private Size calculateMinSize()
        {
            return new Size(innerForm.MinimumSize.Width,
                innerForm.MinimumSize.Height + flowLayoutPanel1.Height + innerViewPanel.Margin.Vertical
                + flowLayoutPanel1.Margin.Vertical);
        }

        private void Form_MinimumSizeChanged(object sender, EventArgs e)
        {
            this.MinimumSize = calculateMinSize();
        }

        private void exportBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = sender as BackgroundWorker;
            var arg = e.Argument as ExportArgs;
            FileStream output = new FileStream(arg.Filename, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[4096];
            long start = 0;
            var length = arg.Input.Length;
            for (long i = length; i > 0; )
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    output.Close();
                    File.Delete(arg.Filename);
                    return;
                }
                int read = (int)Math.Min(buffer.Length, i);
                arg.Input.ReadBytesToBuffer(buffer, start, read);
                start += read;
                i -= read;
                output.Write(buffer, 0, read);
                bw.ReportProgress((int)((100 * start) / length));
            }
            output.Close();
        }

        private void exportBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            exportProgressBar.Value = e.ProgressPercentage;
        }

        private void exportBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportBtn.Text = "Export to file";
            exportBtn.Click -= cancelBtn_Click;
            exportBtn.Click += exportBtn_Click;
            exportProgressBar.Value = 0;
            if (e.Error != null)
            {
                MessageBox.Show("Error occured in export: " + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!e.Cancelled)
            {
                MessageBox.Show("Export completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}