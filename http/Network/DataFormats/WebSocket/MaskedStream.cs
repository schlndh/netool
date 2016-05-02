using System;

namespace Netool.Network.DataFormats.WebSocket
{
    /// <summary>
    /// Stream that masks it's inner stream by XORing it with given mask
    /// </summary>
    /// <remarks>
    /// Used for https://tools.ietf.org/html/rfc6455#section-5.3
    /// Apply a MaskedStream with the same key again to unmask it.
    /// </remarks>
    [Serializable]
    public class MaskedStream : IDataStream
    {
        /// <inheritdoc/>
        public long Length { get { return innerStream.Length; } }

        private IDataStream innerStream;
        private byte[] mask;

        /// <summary>
        ///
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="innerStream"></param>
        /// <exception cref="ArgumentNullException">mask or innerStream</exception>
        /// <exception cref="ArgumentException">mask length is not 4</exception>
        public MaskedStream(byte[] mask, IDataStream innerStream)
        {
            if (mask == null) throw new ArgumentNullException("mask");
            if (innerStream == null) throw new ArgumentNullException("innerStream");
            if (mask.Length != 4) throw new ArgumentException("mask length is not 4!");
            this.mask = new byte[4];
            Array.Copy(mask, this.mask, 4);
            this.innerStream = innerStream;
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            var b = innerStream.ReadByte(index);
            return (byte)(b ^ mask[index % 4]);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            innerStream.ReadBytesToBuffer(buffer, start, length, offset);
            for (long i = 0; i < length; ++i)
            {
                buffer[offset + i] = (byte)(buffer[offset + i] ^ mask[(start + i) % 4]);
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            var c = (IDataStream)innerStream.Clone();
            if (object.ReferenceEquals(c, innerStream)) return this;
            return new MaskedStream(mask, c);
        }

        public override string ToString()
        {
            return "websocketMask(" + innerStream.ToString() + ")";
        }
    }
}