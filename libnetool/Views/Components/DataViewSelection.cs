using Netool.Network.DataFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

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

        /// <summary>
        /// Get a list of possible inner views or null if IsEditor
        /// </summary>
        public IList<IEventView> InnerViews { get { return isEditor ? null : innerViews; } }

        /// <summary>
        /// Get a list of possible inner editors or null if !IsEditor
        /// </summary>
        public IList<IEditorView> InnerEditors { get { return !isEditor ? null : innerEditors; } }

        /// <summary>
        /// Get an index of the selected view/editor.
        /// </summary>
        public int SelectedIndex { get { return innerViewSelect.SelectedIndex; } set { innerViewSelect.SelectedIndex = value; } }

        public DataViewSelection()
        {
            InitializeComponent();
            innerViews = new TypedIListAdapter<IEventView>(innerViewSelect.Items);
            innerEditors = new TypedIListAdapter<IEditorView>(innerViewSelect.Items);
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
            showStream();
        }

        private void showStream()
        {
            innerViewPanel.Controls.Clear();
            if(!isEditor)
            {
                exportBtn.Enabled = stream != null;
                if (currentView == null || stream == null) return;
                currentView.Show(stream);
                innerViewPanel.Embed(currentView.GetForm());
            }
            else
            {
                if (currentEditor == null) return;
                exportBtn.Enabled = true;
                innerViewPanel.Embed(currentEditor.GetForm());
            }
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