using System;
using System.Collections.Generic;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// DataFormat to represent empty data
    /// </summary>
    [Serializable]
    public class EmptyData : IDataStream
    {
        private EmptyData() { }

        private static EmptyData instance;
        /// <summary>
        /// Get EmptyData singleton instance
        /// </summary>
        public static EmptyData Instance { get { if (instance == null) { instance = new EmptyData(); } return instance; } }

        /// <inheritdoc/>
        public long Length { get { return 0; } }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            throw new ArgumentOutOfRangeException();
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            if (length > 0 || start > 0) throw new ArgumentOutOfRangeException();
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }

        public override string ToString()
        {
            return "EmptyData";
        }
    }
}