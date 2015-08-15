using System;
using System.Collections.Generic;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Immutable data format for in-memory byte data
    /// </summary>
    [Serializable]
    public class ByteArray : IInMemoryData
    {
        private IReadOnlyList<byte> arr;
        /// <inheritdoc/>
        public long Length { get { return arr.Count; } }

        /// <summary>
        /// Creates new ByteArray from a copy of the given array
        /// </summary>
        /// <param name="arr"></param>
        public ByteArray(byte[] arr)
        {
            byte[] narr = new byte[arr.Length];
            Array.Copy(arr, narr, arr.Length);
            this.arr = System.Array.AsReadOnly(narr);
        }

        /// <summary>
        /// Creates new ByteArray from a copy of the given array
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public ByteArray(byte[] arr, int start, int length)
        {
            byte[] narr = new byte[length];
            Array.Copy(arr, start, narr, 0, length);
            this.arr = System.Array.AsReadOnly(narr);
        }

        /// <summary>
        /// Creates new ByteArray from a copy of the given list
        /// </summary>
        /// <param name="arr"></param>
        public ByteArray(IReadOnlyList<byte> arr)
        {
            arr = new List<byte>(arr).AsReadOnly();
        }

        /// <summary>
        /// Creates new ByteArray from a copy of the given stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="start"></param>
        /// <param name="length">length to copy, or -1 to copy from start to the end</param>
        public ByteArray(IDataStream stream, long start = 0, long length = -1)
        {
            if (length == -1) length = stream.Length - start;
            // ReadBytes copies data, so no extra copying is required
            arr = System.Array.AsReadOnly(stream.ReadBytes(start, length));
        }

        /// <inheritdoc/>
        public IReadOnlyList<byte> GetBytes()
        {
            return arr;
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            return DefaultIInMemoryDataStream.ReadByte(this, (int) index);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(IList<ArraySegment<byte>> buffers, long start, long length)
        {
            DefaultIInMemoryDataStream.ReadBytesToBuffer(this, buffers, (int)start, (int)length);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }
    }
}