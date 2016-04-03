using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Plugins;
using Netool.Plugins.Http;
namespace Netool.Views
{
    public partial class HttpDataView : BaseForm, IEventView, IEditorView
    {
        private bool isEditor;

        public static string StaticID { get { return "HttpDataView"; } }

        /// <inheritdoc />
        public string ID { get { return StaticID; } }

        private IReadOnlyDictionary<string, IStreamDecoderPlugin> decoders;

        public HttpDataView(IEnumerable<IEventViewPlugin> eventViews, IReadOnlyDictionary<string, IStreamDecoderPlugin> decoders)
        {
            InitializeComponent();
            this.MinimumSize = this.Size;
            isEditor = dataViewSelection.IsEditor = false;
            this.decoders = decoders;
            dataViewSelection.AddEventViews(eventViews, typeof(Event.HexView));
            init();
        }

        public HttpDataView(IEnumerable<IEditorViewPlugin> editors)
        {
            InitializeComponent();
            this.MinimumSize = this.Size;
            isEditor = dataViewSelection.IsEditor = true;
            dataViewSelection.AddEditors(editors, typeof(Editor.HexView));
            init();
        }

        private void init()
        {
            if(!isEditor)
            {
                statusLine.ReadOnly = true;
                headers.ReadOnly = true;
            }
            if (dataViewSelection.SelectedIndex < 0) dataViewSelection.SelectedIndex = 0;
        }

        /// <inheritdoc />
        void IEventView.Show(IDataStream s)
        {
            setValue(s);
        }

        public Form GetForm()
        {
            return this;
        }

        /// <inheritdoc />
        void IEditorView.Clear()
        {
            statusLine.Text = "";
            headers.Rows.Clear();
            dataViewSelection.Stream = null;
        }

        /// <inheritdoc />
        IDataStream IEditorView.GetValue()
        {
            HttpData.Builder builder;
            try
            {
                builder = HttpHeaderParser.ParseStartLine(statusLine.Text + "\r\n");
            }
            catch(InvalidHttpHeaderException)
            {
                MessageBox.Show("Invalid status line", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            foreach(DataGridViewRow row in headers.Rows)
            {
                if (row.Cells[0].Value == null || row.Cells[1].Value == null) continue;
                var key = row.Cells[0].Value.ToString();
                var val = row.Cells[1].Value.ToString();
                if(!string.IsNullOrEmpty(key))
                {
                    builder.AddHeader(key, val);
                }
            }
            IDataStream payload = dataViewSelection.Stream;
            return builder.CreateAndClear(payload);
        }

        /// <inheritdoc />
        void IEditorView.SetValue(IDataStream s)
        {
            setValue(s);
        }

        private void setValue(IDataStream s)
        {
            this.headers.Rows.Clear();
            var data = s as HttpData;
            if (data != null)
            {
                this.Visible = true;
                statusLine.Text = data.StatusLine;
                foreach (var key in data.HeaderKeys)
                {
                    this.headers.Rows.Add(new object[] { key, data.Headers[key] });
                }

                IDataStream innerData = data.BodyData;
                if(!isEditor && (data.Headers.ContainsKey("Transfer-Encoding") || data.Headers.ContainsKey("Content-Encoding")))
                {
                    var usedDecoders = new List<IStreamWrapper>();
                    List<string> encodings = new List<string>();
                    addEncodings(data, "Content-Encoding", encodings);
                    addEncodings(data, "Transfer-Encoding", encodings);
                    for (int i = encodings.Count - 1; i > -1; --i)
                    {
                        IStreamDecoderPlugin decoder = null;
                        if (decoders.TryGetValue(encodings[i].Trim(), out decoder))
                        {
                            usedDecoders.Add(decoder.CreateWrapper());
                        }
                        else break;
                    }
                    if(usedDecoders.Count > 0)
                    {
                        int i = 0;
                        foreach (var v in dataViewSelection.InnerViews)
                        {
                            var ve = v as Views.Event.EmbeddingEventViewWrapper;
                            if (ve != null)
                            {
                                var vs = ve.View as StreamWrapperView;
                                if(vs != null)
                                {
                                    vs.UsedWrappers = usedDecoders;
                                    dataViewSelection.SelectedIndex = i;
                                    break;
                                }
                            }
                            ++i;
                        }
                    }
                }

                dataViewSelection.Stream = innerData;
            }
            else
            {
                throw new UnsupportedDataStreamException();
            }
        }

        private void addEncodings(HttpData data, string header, List<string> encodings)
        {
            if (data.Headers.ContainsKey(header))
            {
                encodings.AddRange(data.Headers[header].ToLower().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private Size calculateMinSize()
        {
            var dist = splitContainer1.SplitterDistance;
            var size = dataViewSelection.MinimumSize;
            size.Height += dist + splitContainer1.Panel1.Margin.Vertical
                + flowLayoutPanel1.Height + flowLayoutPanel1.Margin.Vertical + dataViewSelection.Margin.Vertical
                + splitContainer1.SplitterWidth;
            size.Width = Math.Max(size.Width, 480);
            return size;
        }

        private void dataViewSelection_MinimumSizeChanged(object sender, EventArgs e)
        {
            MinimumSize = calculateMinSize();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            dataViewSelection_MinimumSizeChanged(dataViewSelection, EventArgs.Empty);
            // this seems to be necessary to resize the split container again
            splitContainer1.Dock = DockStyle.Fill;
        }
    }
}
