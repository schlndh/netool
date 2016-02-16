using System;

namespace Netool.Network.DataFormats.WebSocket
{
    [Serializable]
    public class WebSocketMessage : IDataStream
    {
        /// <inheritdoc/>
        public long Length { get { return binaryStream.Length; } }

        public enum OpcodeType : byte
        {
            Continuation, Text, Binary, Undefined3, Undefined4, Undefined5, Undefined6, Undefined7,
            Close, Ping, Pong, UndefinedB, UndefinedC, UndefinedD, UndefinedE, UndefinedF
        };

        public class Parser
        {
            private int headerLength;
            private WebSocketMessage msg = new WebSocketMessage();

            /// <summary>
            /// Parse the header from data stream
            /// </summary>
            /// <param name="s"></param>
            /// <returns>length of missing payload data (negative if there are more data than neccessary)</returns>
            public long ParseHeader(IDataStream s)
            {
                headerLength = 2;
                var len = s.Length;
                if (len < 2) throw new ArgumentException("Stream too short to contain the header!");
                byte b1, b2;
                b1 = s.ReadByte(0);
                b2 = s.ReadByte(1);
                msg.FIN = (b1 & 0x80) == 0x80;
                msg.RSV1 = (b1 & 0x40) == 0x40;
                msg.RSV2 = (b1 & 0x20) == 0x20;
                msg.RSV3 = (b1 & 0x10) == 0x10;
                msg.Opcode = (OpcodeType)(b1 & 0x0F);
                msg.MASK = (b2 & 0x80) == 0x80;
                msg.PayloadLength = b2 & ~0x80;
                byte[] buffer;
                if (msg.PayloadLength == 126)
                {
                    if (len < 4) throw new ArgumentException("Stream too short to contain the header!");
                    buffer = new byte[2];
                    s.ReadBytesToBuffer(buffer, 2, 2);
                    if (BitConverter.IsLittleEndian)
                    {
                        b1 = buffer[0];
                        buffer[0] = buffer[1];
                        buffer[1] = b1;
                    }
                    msg.PayloadLength = BitConverter.ToUInt16(buffer, 0);
                    headerLength += 2;
                }
                else if (msg.PayloadLength == 127)
                {
                    if (len < 10) throw new ArgumentException("Stream too short to contain the header!");
                    buffer = new byte[8];
                    s.ReadBytesToBuffer(buffer, 2, 8);
                    if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
                    // lets hope no one will ever send a websocket message that long that this actually matters.
                    msg.PayloadLength = (long)BitConverter.ToUInt64(buffer, 0);
                    headerLength += 8;
                }
                if (msg.MASK)
                {
                    buffer = new byte[4];
                    s.ReadBytesToBuffer(buffer, headerLength, 4);
                    msg.MaskingKey = buffer;
                    headerLength += 4;
                }
                return headerLength + msg.PayloadLength - len;
            }

            /// <summary>
            /// Closes the builder - don't use it anymore
            /// </summary>
            /// <param name="s">binary representation of the whole message (including payload)</param>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException">stream not long enough</exception>
            public WebSocketMessage Close(IDataStream s)
            {
                msg.binaryStream = s;
                msg.InnerData = new StreamSegment(s, headerLength, msg.PayloadLength);
                if (msg.MASK)
                {
                    msg.InnerData = new MaskedStream(msg.MaskingKey, msg.InnerData);
                }
                return msg;
            }
        }

        public bool FIN { get; protected set; }
        public bool RSV1 { get; protected set; }
        public bool RSV2 { get; protected set; }
        public bool RSV3 { get; protected set; }
        public OpcodeType Opcode { get; protected set; }
        public bool MASK { get; protected set; }
        public long PayloadLength { get; protected set; }
        public byte[] MaskingKey { get; protected set; }
        public IDataStream InnerData { get; protected set; }
        private IDataStream binaryStream;

        /// <summary>
        ///
        /// </summary>
        /// <param name="fin"></param>
        /// <param name="opcode"></param>
        /// <param name="maskingKey">either null which will set MASK bit to 0, or byte array of 4 items which will set MASK to 1</param>
        /// <param name="payload">unmasked data</param>
        /// <param name="rsv1"></param>
        /// <param name="rsv2"></param>
        /// <param name="rsv3"></param>
        public WebSocketMessage(bool fin, OpcodeType opcode, byte[] maskingKey, IDataStream payload, bool rsv1 = false, bool rsv2 = false, bool rsv3 = false)
        {
            if (MaskingKey != null && MaskingKey.Length != 4) throw new ArgumentException("MaskingKey.Length != 4");
            MaskingKey = maskingKey;
            FIN = fin;
            RSV1 = rsv1;
            RSV2 = rsv2;
            RSV3 = rsv3;
            Opcode = opcode;
            MASK = MaskingKey != null;
            PayloadLength = payload.Length;
            InnerData = payload;
            var buffer = new byte[calculateHeaderLength()];
            buffer[0] = (byte)((FIN ? 128 : 0) + (RSV1 ? 64 : 0) + (RSV2 ? 32 : 0) + (RSV3 ? 16 : 0) + ((byte)Opcode));
            int i = 2;
            if (PayloadLength > 125)
            {
                if (PayloadLength <= UInt16.MaxValue)
                {
                    buffer[1] = 126;
                    var bytes = BitConverter.GetBytes((UInt16)PayloadLength);
                    if (BitConverter.IsLittleEndian)
                    {
                        byte b0 = bytes[0];
                        bytes[0] = bytes[1];
                        bytes[1] = b0;
                    }
                    buffer[2] = bytes[0];
                    buffer[3] = bytes[1];
                    i = 4;
                }
                else
                {
                    buffer[1] = 127;
                    var bytes = BitConverter.GetBytes((UInt64)PayloadLength);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }
                    Array.Copy(bytes, 0, buffer, 2, 8);
                    i = 10;
                }
            }
            else
            {
                buffer[1] = (byte)PayloadLength;
            }
            buffer[1] += (byte)(MASK ? 128 : 0);
            if (MASK)
            {
                Array.Copy(MaskingKey, 0, buffer, i, 4);
            }
            var header = new ByteArray(buffer);
            if (MASK)
            {
                payload = new MaskedStream(maskingKey, payload);
            }
            var res = new StreamList();
            res.Add(header);
            res.Add(payload);
            if (PayloadLength < 4096)
            {
                binaryStream = new ByteArray(res);
            }
            else
            {
                binaryStream = res;
            }
        }

        private WebSocketMessage()
        {
        }

        protected int calculateHeaderLength()
        {
            return 2 + (PayloadLength <= 125 ? 0 : (PayloadLength <= UInt16.MaxValue ? 2 : 8)) + (MASK ? 4 : 0);
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            return binaryStream.ReadByte(index);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            binaryStream.ReadBytesToBuffer(buffer, start, length, offset);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }
    }
}