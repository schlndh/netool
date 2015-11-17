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
        /// <inheritdoc cref="IDataStreamHelpers.ReadByteArgsCheck" select="exception"/>
        byte ReadByte(long index);

        /// <summary>
        /// Copies bytes to buffer, used for more efficient access to data than one-by-one with ReadByte
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="length">-1 means from start to the end of the stream (or int.MaxValue)</param>
        /// <param name="offset">offset to begin writing at to the target buffer</param>
        /// <inheritdoc cref="IDataStreamHelpers.ReadBytesToBufferArgsCheck" select="exception"/>
        void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0);
    }

    public static class IDataStreamExtensions
    {
        /// <summary>
        /// Reads bytes and returns them as a byte array. This method must perform a copy.
        /// </summary>
        /// <param name="s"></param>
        /// <inheritdoc cref="IDataStream.ReadBytesToBuffer" select="param[@name='start']|param[@name='length']|exception"/>
        /// <inheritdoc cref="IDataStreamHelpers.ReadBytesToBufferArgsCheck" select="exception"/>
        public static byte[] ReadBytes(this IDataStream s, long start = 0, int length = -1)
        {
            if (length == -1) length = (int) Math.Min(int.MaxValue, s.Length - start);
            var ret = new byte[length];
            s.ReadBytesToBuffer(ret, start, length);
            return ret;
        }
    }

    public static class IDataStreamHelpers
    {
        /// <summary>
        /// Checks parameters passed to ReadBytesToBuffer and sets correct length if necessary (-1).
        /// </summary>
        /// <remarks>
        /// This is helper method is here to assure that IDataStreams handle ReadBytesToBuffer arguments consistently - eg. properly handling length = -1.
        /// Call this method as a first thing in your IDataStream unless you have a good reason not to.
        /// </remarks>
        /// <param name="s"></param>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <exception cref="ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="IndexOutOfRangeException">Buffer not large enough or wrong index.</exception>
        public static void ReadBytesToBufferArgsCheck(IDataStream s, byte[] buffer, long start, ref int length, long offset)
        {
            var len = s.Length;
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (length == -1) length = (int)Math.Min(int.MaxValue, len - start);
            if (buffer.Length - offset < length) throw new IndexOutOfRangeException("Buffer not large enough or wrong index.");
            if (len - start < length) throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Checks parameters passed to ReadByte
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static void ReadByteArgsCheck(IDataStream s, long index)
        {
            if (index < 0 || index >= s.Length) throw new IndexOutOfRangeException();
        }
    }
}