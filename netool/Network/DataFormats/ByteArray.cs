using System;
namespace Netool.Network.DataFormats
{
    public class ByteArray : IByteArrayConvertible
    {
        private byte[] arr;

        public ByteArray(byte[] arr)
        {
            this.arr = arr;
        }

        public byte[] ToByteArray()
        {
            return arr;
        }

        public object Clone()
        {
            byte[] narr = new byte[arr.Length];
            Array.Copy(arr, narr, arr.Length);
            return new ByteArray(narr);
        }
    }
}