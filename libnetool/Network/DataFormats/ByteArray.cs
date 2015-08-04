using System;
using System.Collections.Generic;

namespace Netool.Network.DataFormats
{
    [Serializable]
    public class ByteArray : IInMemoryData
    {
        private byte[] arr;
        public long Length { get { return arr.Length; } }

        public ByteArray(byte[] arr)
        {
            this.arr = arr;
        }

        public IReadOnlyList<byte> GetBytes()
        {
            return System.Array.AsReadOnly(arr);
        }

        public byte ReadByte(long index)
        {
            return DefaultIInMemoryDataStream.ReadByte(this, (int) index);
        }


        public void ReadBytesToBuffer(long start, long length, IList<ArraySegment<byte>> buffers)
        {
            DefaultIInMemoryDataStream.ReadBytesToBuffer(this, (int)start, (int)length, buffers);
        }

        public object Clone()
        {
            byte[] narr = new byte[arr.Length];
            Array.Copy(arr, narr, arr.Length);
            return new ByteArray(narr);
        }
    }
}