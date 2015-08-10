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
            Array.Copy(arr, start, narr, 0, arr.Length);
            this.arr = System.Array.AsReadOnly(narr);
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
        public void ReadBytesToBuffer(long start, long length, IList<ArraySegment<byte>> buffers)
        {
            DefaultIInMemoryDataStream.ReadBytesToBuffer(this, (int)start, (int)length, buffers);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }
    }
}