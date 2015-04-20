using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
