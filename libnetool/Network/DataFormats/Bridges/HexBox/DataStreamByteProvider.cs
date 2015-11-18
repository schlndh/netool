using Be.Windows.Forms;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Utils;
using System;

namespace Netool.Network.DataFormats.Bridges.HexBox
{
    /// <summary>
    /// IDataStream byte provider for HexBox widget
    /// </summary>
    /// <remarks>
    /// This class doesn't report any changes done to the underlaying IDataStream
    /// </remarks>
    public class DataStreamByteProvider : IByteProvider
    {
        private IDataStream stream;
        private ByteCache cache = new ByteCache(4096);
        public long Length { get { return stream.Length; } }
        public event EventHandler LengthChanged;
        public event EventHandler Changed;

        public DataStreamByteProvider(IDataStream stream)
        {
            this.stream = stream;
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            byte ret;
            if (!cache.TryReadByte(index, out ret))
            {
                ByteCache.FillCache callback = delegate(byte[] buffer, out long cacheStart, out int cacheLength)
                {
                    // the file is more likely to be read from the beginning to the end
                    cacheStart = Math.Max(0, index - 512);
                    cacheLength = (int)Math.Min(buffer.Length, Math.Min(int.MaxValue, stream.Length - cacheStart));
                    stream.ReadBytesToBuffer(buffer, cacheStart, cacheLength);
                    ret = buffer[index - cacheStart];
                };
                cache.Cache(callback);
            }
            return ret;
        }

        /// <inheritdoc/>
        public void ApplyChanges()
        {
            throw new NotSupportedException("DataStreamByteProvider.ApplyChanges");
        }

        /// <inheritdoc/>
        public void DeleteBytes(long index, long length)
        {
            throw new NotSupportedException("DataStreamByteProvider.DeleteBytes");
        }

        /// <inheritdoc/>
        public bool HasChanges()
        {
            return false;
        }

        /// <inheritdoc/>
        public void InsertBytes(long index, byte[] bs)
        {
            throw new NotSupportedException("DataStreamByteProvider.InsertBytes");
        }

        /// <inheritdoc/>
        public bool SupportsDeleteBytes()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool SupportsInsertBytes()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool SupportsWriteByte()
        {
            return false;
        }

        /// <inheritdoc/>
        public void WriteByte(long index, byte value)
        {
            throw new NotSupportedException("DataStreamByteProvider.WriteByte");
        }
    }
}
