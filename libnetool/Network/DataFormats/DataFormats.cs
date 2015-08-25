using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Basic interface for request/response data
    /// </summary>
    /// <remarks>
    /// If data is mutable then clone should return deep-copy.
    /// All classes implementing this interface must be serializable, it is required for event logging.
    /// </remarks>
    public interface IDataStream : ICloneable
    {
        /// <summary>
        /// Get stream length in bytes
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Read single byte
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        byte ReadByte(long index);

        /// <summary>
        /// Copies bytes to buffer, used for more efficient access to data than one-by-one with ReadByte
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="length">-1 means from start to the end of the stream (or int.MaxValue)</param>
        /// <param name="offset">offset to begin writing at to the target buffer</param>
        void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0);
    }

    public static class IDataStreamExtensions
    {
        /// <summary>
        /// Reads bytes and returns them as a byte array. This method must perform a copy.
        /// </summary>
        /// <param name="s"></param>
        /// <inheritdoc cref="IDataStream.ReadBytesToBuffer" select="param[@name='start']|param[@name='length']|exception"/>
        public static byte[] ReadBytes(this IDataStream s, long start = 0, int length = -1)
        {
            if (length == -1) length = (int) Math.Min(int.MaxValue, s.Length - start);
            var ret = new byte[length];
            s.ReadBytesToBuffer(ret, start, length);
            return ret;
        }
    }
}