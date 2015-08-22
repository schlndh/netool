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
namespace Netool.Views
{
    public partial class HttpDataView : Form, IEventView, IEditorView
    {
        IEventView innerEventView = null;
        IEditorView innerEditorView = null;
        private bool isEditor;
        /// <inheritdoc />
        public string ID { get { return "HttpDataView"; } }

        public HttpDataView(bool isEditor)
        {
            this.isEditor = isEditor;
            InitializeComponent();

            // TODO: offer all available EventViews
            if (isEditor)
            {
                innerViewSelect.Items.Add(new Editor.HexView());
            }
            else
            {
                innerViewSelect.Items.Add(new Event.HexView());
                statusLine.ReadOnly = true;
                headers.ReadOnly = true;
            }

            if (innerViewSelect.SelectedIndex < 0) innerViewSelect.SelectedIndex = 0;
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

        private void innerViewsSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(innerViewSelect.SelectedIndex > -1)
            {
                if(isEditor)
                {
                    innerEditorView = ((IEditorView)innerViewSelect.SelectedItem);
                    innerDataView.Embed(innerEditorView.GetForm());
                }
                else
                {
                    innerEventView = ((IEventView)innerViewSelect.SelectedItem);
                }
            }
            else
            {
                if(isEditor)
                {
                    innerEditorView = null;
                }
                else
                {
                    innerEventView = null;
                }
            }
        }

        /// <inheritdoc />
        void IEditorView.Clear()
        {
            statusLine.Text = "";
            headers.Rows.Clear();
            if(innerEditorView != null)
            {
                innerEditorView.Clear();
            }
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
            IDataStream payload = null;
            if(innerEditorView != null)
            {
                payload = innerEditorView.GetValue();
            }
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
                // placeholder
                if (data.Headers.ContainsKey("Content-Encoding") && data.Headers["Content-Encoding"] == "gzip")
                {
                    var decompressed = new System.IO.Compression.GZipStream(new ToStream(data.BodyData), System.IO.Compression.CompressionMode.Decompress);
                    innerData = FromStream.ToIDataStream(decompressed);
                }
                if (!isEditor && innerEventView != null)
                {
                    innerEventView.Show(innerData);
                    innerDataView.Embed(innerEventView.GetForm());
                }
                if(isEditor && innerEditorView != null)
                {
                    innerEditorView.SetValue(innerData);
                    innerDataView.Embed(innerEditorView.GetForm());
                }
            }
            else
            {
                this.Visible = false;
                MessageBox.Show("Unsupported data type given.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
