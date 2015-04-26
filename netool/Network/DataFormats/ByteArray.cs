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
    }
}