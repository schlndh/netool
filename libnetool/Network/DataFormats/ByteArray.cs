using System;
using System.Collections.Generic;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Immutable data format for in-memory byte data
    /// </summary>
    [Serializable]
    public class ByteArray : IDataStream
    {
        private byte[] arr;
        /// <inheritdoc/>
        public long Length { get { return arr.Length; } }

        /// <summary>
        /// Creates new ByteArray from a copy of the given array
        /// </summary>
        /// <param name="arr"></param>
        public ByteArray(byte[] arr)
        {
            this.arr = new byte[arr.Length];
            Array.Copy(arr, this.arr, arr.Length);
        }

        /// <summary>
        /// Creates new ByteArray from a copy of the given array
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public ByteArray(byte[] arr, int start, int length)
        {
            this.arr = new byte[length];
            Array.Copy(arr, start, this.arr, 0, length);
        }

        /// <summary>
        /// Creates new ByteArray from a copy of the given list
        /// </summary>
        /// <param name="arr"></param>
        public ByteArray(ICollection<byte> arr)
        {
            this.arr = new byte[arr.Count];
            arr.CopyTo(this.arr, 0);
        }

        /// <summary>
        /// Creates new ByteArray from a copy of the given stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="start"></param>
        /// <param name="length">length to copy, or -1 to copy from start to the end</param>
        public ByteArray(IDataStream stream, long start = 0, int length = -1)
        {
            if (length == -1) length = (int)Math.Min(int.MaxValue, stream.Length - start);
            // ReadBytes copies data, so no extra copying is required
            arr = stream.ReadBytes(start, length);
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            return arr[index];
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            Array.Copy(arr, start, buffer, offset, length);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }
    }
}