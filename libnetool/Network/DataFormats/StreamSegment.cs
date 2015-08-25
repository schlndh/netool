using System;
using System.Collections.Generic;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Data structure similar to ArraySegment
    /// </summary>
    /// <remarks>
    /// This type is thread-safe only if underlying stream is thread-safe, the same applies to immutability.
    /// </remarks>
    [Serializable]
    public struct StreamSegment : IDataStream
    {
        private long count;
        private long offset;
        private IDataStream stream;
        /// <summary>
        /// Get the underlaying data strea,
        /// </summary>
        public IDataStream Stream { get { return stream; } }

        public long Offset { get { return offset; } }
        /// <inheritdoc/>
        public long Length { get { return count; } }

        /// <summary>
        /// Creates new StreamSegment
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="count">-1 to make the segment from offset to the end of the stream</param>
        public StreamSegment(IDataStream stream, long offset = 0, long count = -1)
        {
            if (count == -1) count = stream.Length - offset;
            this.stream = stream;
            this.offset = offset;
            this.count = count;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This doesn't clone the underlying stream
        /// </remarks>
        public object Clone()
        {
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">wrong index</exception>
        public byte ReadByte(long index)
        {
            if (index > offset || index < 0) throw new ArgumentOutOfRangeException("wrong index");
            return stream.ReadByte(offset + index);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">wrong index or length</exception>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            if (length == -1) length = (int) Math.Min(int.MaxValue, Length - start);
            if (start + length > count || start < 0 || length < 0) throw new ArgumentOutOfRangeException("wrong index or length");
            stream.ReadBytesToBuffer(buffer, this.offset + start, length, offset);
        }
    }
}