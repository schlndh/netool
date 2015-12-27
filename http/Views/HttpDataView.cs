using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Plugins;
using Netool.Plugins.Http;
namespace Netool.Views
{
    public partial class HttpDataView : Form, IEventView, IEditorView
    {
        private bool isEditor;
        /// <inheritdoc />
        public string ID { get { return "HttpDataView"; } }

        private IReadOnlyDictionary<string, IStreamDecoderPlugin> decoders;

        public HttpDataView(IEnumerable<IEventViewPlugin> eventViews, IReadOnlyDictionary<string, IStreamDecoderPlugin> decoders)
        {
            InitializeComponent();
            isEditor = dataViewSelection.IsEditor = false;
            this.decoders = decoders;
            foreach(var pl in eventViews)
            {
                foreach(var v in pl.CreateEventViews())
                {
                    dataViewSelection.InnerViews.Add(v);
                    // prevent inifinite embedding
                    if(v is Event.HexView)
                    {
                        dataViewSelection.SelectedIndex = dataViewSelection.InnerViews.Count - 1;
                    }
                }
            }
            init();
        }

        public HttpDataView(IEnumerable<IEditorViewPlugin> editors)
        {
            InitializeComponent();
            isEditor = dataViewSelection.IsEditor = true;
            foreach (var pl in editors)
            {
                foreach (var v in pl.CreateEditorViews())
                {
                    dataViewSelection.InnerEditors.Add(v);
                    // prevent inifinite embedding
                    if (v is Editor.HexView)
                    {
                        dataViewSelection.SelectedIndex = dataViewSelection.InnerEditors.Count - 1;
                    }
                }
            }
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
            dataViewSelection.Stream = s;
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
                if(!isEditor && data.Headers.ContainsKey("Transfer-Encoding"))
                {
                    var usedDecoders = new List<IStreamWrapper>();
                    var transferEncodings = data.Headers["Transfer-Encoding"].ToLower().Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
                    for(int i = transferEncodings.Length - 1; i > -1; --i)
                    {
                        IStreamDecoderPlugin decoder = null;
                        if (decoders.TryGetValue(transferEncodings[i].Trim(), out decoder))
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
                this.Visible = false;
                MessageBox.Show("Unsupported data type given.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
