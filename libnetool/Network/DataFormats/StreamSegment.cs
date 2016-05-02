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
        /// <exception cref="ArgumentOutOfRangeException">wrong offset or count</exception>
        /// <exception cref="ArgumentNullException">stream</exception>
        /// <remarks>
        /// It's optimalized for recursive StreamSegments:
        /// StreamSegment(StreamSegment(s, 1, 9), 2, 3) will be the same as StreamSegment(s, 3, 3)
        /// </remarks>
        public StreamSegment(IDataStream stream, long offset = 0, long count = -1)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (offset < 0) offset = stream.Length + offset;
            if (offset < 0 || offset > stream.Length) throw new ArgumentOutOfRangeException("invalid offset");
            if (count == -1) count = stream.Length - offset;
            if (count < 0 || offset + count > stream.Length) throw new ArgumentOutOfRangeException("invalid count");
            this.count = count;

            // optimalization for recursive StreamSegments
            if(stream.GetType() == typeof(StreamSegment))
            {
                var s = (StreamSegment)stream;
                this.stream = s.Stream;
                this.offset = s.Offset + offset;
            }
            else
            {
                this.stream = stream;
                this.offset = offset;
            }
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
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            return stream.ReadByte(offset + index);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            if (length == -1) length = (int) Math.Min(int.MaxValue, Length - start);
            if (start + length > count || start < 0 || length < 0) throw new ArgumentOutOfRangeException("wrong index or length");
            stream.ReadBytesToBuffer(buffer, this.offset + start, length, offset);
        }

        public override string ToString()
        {
            return "segment(" + stream.ToString() + ", " + offset + ", " + count + ")";
        }
    }
}