using Netool.Network.DataFormats;
using Netool.Network.DataFormats.WebSocket;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Netool.Views
{
    public partial class WebSocketMessageView : Form, IEventView, IEditorView
    {
        public static string StaticID { get { return "WebSocketView"; } }

        /// <inheritdoc/>
        public string ID { get { return StaticID; } }

        private Random randomGen = new Random();
        private byte[] randKey = new byte[4];

        private bool isEditor;

        public WebSocketMessageView(IEnumerable<IEventViewPlugin> eventViews)
        {
            InitializeComponent();
            isEditor = dataViewSelection.IsEditor = false;
            dataViewSelection.AddEventViews(eventViews, typeof(Event.HexView));
            init();
        }

        public WebSocketMessageView(IEnumerable<IEditorViewPlugin> editorViews)
        {
            InitializeComponent();
            isEditor = dataViewSelection.IsEditor = true;
            dataViewSelection.AddEditors(editorViews, typeof(Editor.HexView));
            payloadLengthLabel.Visible = payloadLengthTextBox.Visible = false;
            init();
        }

        private void init()
        {
            opcodeComboBox.Items.AddRange(Enum.GetNames(typeof(WebSocketMessage.OpcodeType)));
            opcodeComboBox.SelectedItem = WebSocketMessage.OpcodeType.Binary.ToString();
            maskCheckBox_CheckedChanged(maskCheckBox, null);
            if (!isEditor)
            {
                finCheckBox.Enabled = rsv1CheckBox.Enabled =
                    rsv2CheckBox.Enabled = rsv3CheckBox.Enabled =
                    maskCheckBox.Enabled = maskTextBox.Enabled =
                    payloadLengthTextBox.Enabled = opcodeComboBox.Enabled = false;
            }
        }

        private void maskCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var s = (CheckBox)sender;
            maskTextBox.Visible = s.Checked;
            maskNoteLabel.Visible = s.Checked;
            if (maskTextBox.Visible)
            {
                randomGen.NextBytes(randKey);
                maskTextBox.Text = maskingKeyToString(randKey);
            }
        }

        /// <inheritdoc/>
        void IEventView.Show(IDataStream s)
        {
            setValue(s);
        }

        /// <inheritdoc/>
        public Form GetForm()
        {
            return this;
        }

        /// <inheritdoc/>
        void IEditorView.Clear()
        {
            finCheckBox.Checked = true;
            rsv1CheckBox.Checked = false;
            rsv2CheckBox.Checked = false;
            rsv3CheckBox.Checked = false;
            maskCheckBox.Checked = false;
            dataViewSelection.Stream = null;
        }

        /// <inheritdoc/>
        IDataStream IEditorView.GetValue()
        {
            byte[] maskingKey = null;
            if (maskCheckBox.Checked)
            {
                UInt32 key;
                if (!TryParseUInt32(maskTextBox.Text, out key))
                {
                    maskTextBox.Focus();
                    throw new ValidationException("Invalid masking key entered! Enter valid key and try again. Valid key formats are hexadecimal prefixed with 0x or decimal, both  32-bit unsigned.");
                }
                maskingKey = BitConverter.GetBytes(key);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(maskingKey);
                }
            }
            var opcode = (WebSocketMessage.OpcodeType)Enum.Parse(typeof(WebSocketMessage.OpcodeType), opcodeComboBox.SelectedItem.ToString());
            return new WebSocketMessage(finCheckBox.Checked, opcode, maskingKey, dataViewSelection.Stream, rsv1CheckBox.Checked, rsv2CheckBox.Checked, rsv3CheckBox.Checked);
        }

        private static bool TryParseUInt32(string input, out UInt32 output)
        {
            output = 0;
            if (input.Length > 1 && input[1] == 'x')
            {
                return UInt32.TryParse(input.Substring(2), NumberStyles.HexNumber, null, out output);
            }
            return UInt32.TryParse(input, out output);
        }

        /// <inheritdoc/>
        void IEditorView.SetValue(IDataStream s)
        {
            setValue(s);
        }

        private void setValue(IDataStream s)
        {
            var msg = s as WebSocketMessage;
            if (msg != null)
            {
                finCheckBox.Checked = msg.FIN;
                rsv1CheckBox.Checked = msg.RSV1;
                rsv2CheckBox.Checked = msg.RSV2;
                rsv3CheckBox.Checked = msg.RSV3;
                maskCheckBox.Checked = msg.MASK;
                if (msg.MaskingKey != null)
                {
                    var key = msg.MaskingKey;
                    maskTextBox.Text = maskingKeyToString(key);
                }
                payloadLengthTextBox.Text = msg.PayloadLength.ToString();
                opcodeComboBox.SelectedItem = msg.Opcode.ToString();
                dataViewSelection.Stream = msg.InnerData;
            }
            else
            {
                throw new UnsupportedDataStreamException();
            }
        }

        private static string maskingKeyToString(byte[] key)
        {
            return "0x" + key[0].ToString("X") + key[1].ToString("X") + key[2].ToString("X") + key[3].ToString("X");
        }
    }
}