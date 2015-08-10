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
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="buffers">The sum of lengths of individual subbuffers must be at least length</param>
        void ReadBytesToBuffer(long start, long length, IList<ArraySegment<byte>> buffers);
    }

    /// <summary>
    /// Should be used for data that are already all in memory as a byte array
    /// </summary>
    /// <remarks>
    /// </remarks>
    public interface IInMemoryData : IDataStream
    {
        /// <summary>
        /// Get whole data at once
        /// </summary>
        IReadOnlyList<byte> GetBytes();
    }

    public static class IDataStreamExtensions
    {
        /// <summary>
        /// shortcut method when you don't already have buffer allocated
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public static byte[] ReadBytes(this IDataStream s, long start, long length)
        {
            var ret = new byte[length];
            s.ReadBytesToBuffer(start, length, new List<ArraySegment<byte>> { new ArraySegment<byte>(ret) });
            return ret;
        }
    }

    /// <summary>
    /// You can call methods of this class to easily add default IDataStream members to your IInMemoryData class
    /// </summary>
    /// <remarks>
    /// Note that unlike IDataStream interface, methods of this class take int indexes instead of long ones.
    /// This is because IList only supports int indexes and IInMemoryData shouldn't be that big anyway.
    /// </remarks>
    public static class DefaultIInMemoryDataStream
    {
        /// <summary>
        /// Default implementation of ReadByte for IInMemoryData
        /// </summary>
        /// <param name="d"></param>
        /// <param name="index"></param>
        /// <example>
        /// Example default implementation for your class
        /// <code language="C#">
        /// public byte ReadByte(long index)
        /// {
        ///     return DefaultIInMemoryDataStream.ReadByte(this, (int) index);
        /// }
        /// </code>
        /// </example>
        public static byte ReadByte(IInMemoryData d, int index)
        {
            var l = d.GetBytes();
            return l[index];
        }

        /// <summary>
        /// Default implementation of ReadBytesToBuffer for IInMemoryData
        /// </summary>
        /// <param name="d"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="buffers"></param>
        /// <example>
        /// Example default implementation for your class
        /// <code language="C#">
        /// <![CDATA[
        /// public void ReadBytesToBuffer(long start, long length, IList<ArraySegment<byte>> buffers)
        /// {
        ///     DefaultIInMemoryDataStream.ReadBytesToBuffer(this, (int)start, (int)length, buffers);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static void ReadBytesToBuffer(IInMemoryData d, int start, int length, IList<ArraySegment<byte>> buffers)
        {
            var l = d.GetBytes();
            int bufferIndex = 0;
            var buffer = buffers[bufferIndex];
            var end = start + length;
            var currentBufferSize = 0;
            for (int i = start; i < end; ++i, ++currentBufferSize)
            {
                // buffer full
                while(currentBufferSize == buffer.Count)
                {
                    ++bufferIndex;
                    buffer = buffers[bufferIndex];
                    currentBufferSize = 0;
                }
                buffer.Array[buffer.Offset + currentBufferSize] = l[i];
            }
        }

    }
}