using System;
using System.Collections.Generic;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// DataFormat to represent empty data
    /// </summary>
    [Serializable]
    public class EmptyData : IInMemoryData
    {
        /// <inheritdoc/>
        public long Length { get { return 0; } }

        /// <inheritdoc/>
        public IReadOnlyList<byte> GetBytes()
        {
            return new List<byte>().AsReadOnly();
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            throw new ArgumentOutOfRangeException();
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(IList<ArraySegment<byte>> buffers, long start, long length)
        {
            if (length > 0) throw new ArgumentOutOfRangeException();
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }
    }
}