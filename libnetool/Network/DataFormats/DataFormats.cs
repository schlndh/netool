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
        /// <param name="buffers">The sum of lengths of individual subbuffers must be at least length</param>
        /// <param name="start"></param>
        /// <param name="length">-1 means from start to the end of the stream</param>
        void ReadBytesToBuffer(IList<ArraySegment<byte>> buffers, long start = 0, long length = -1);
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
        /// Read bytes to provided byte array
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arr"></param>
        /// <inheritdoc cref="IDataStream.ReadBytesToBuffer" select="param[@name='start']|param[@name='length']|exception"/>
        public static void ReadBytesToByteArray(this IDataStream s, byte[] arr, long start = 0, long length = -1)
        {
            s.ReadBytesToBuffer(new List<ArraySegment<byte>> { new ArraySegment<byte>(arr) }, start, length);
        }

        /// <summary>
        /// Reads bytes and returns them as a byte array. This method must perform a copy.
        /// </summary>
        /// <param name="s"></param>
        /// <inheritdoc cref="IDataStream.ReadBytesToBuffer" select="param[@name='start']|param[@name='length']|exception"/>
        public static byte[] ReadBytes(this IDataStream s, long start = 0, long length = -1)
        {
            if (length == -1) length = s.Length - start;
            var ret = new byte[length];
            s.ReadBytesToBuffer(new List<ArraySegment<byte>> { new ArraySegment<byte>(ret) }, start, length);
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
        /// <exception cref="ArgumentOutOfRangeException">index >= d.Length</exception>
        public static byte ReadByte(IInMemoryData d, int index)
        {
            if (d.Length <= index) throw new ArgumentOutOfRangeException();
            var l = d.GetBytes();
            return l[index];
        }

        /// <summary>
        /// Default implementation of ReadBytesToBuffer for IInMemoryData
        /// </summary>
        /// <param name="d"></param>
        /// <inheritdoc cref="IDataStream.ReadBytesToBuffer" select="param[@name='start']|param[@name='length']|exception"/>
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
        /// <exception cref="ArgumentOutOfRangeException"><![CDATA[d.Length < start + length]]></exception>
        /// <exception cref="ArgumentNullException">buffers is null</exception>
        public static void ReadBytesToBuffer(IInMemoryData d, IList<ArraySegment<byte>> buffers, int start = 0, int length = -1)
        {
            if (d.Length < start + length) throw new ArgumentOutOfRangeException();
            if (buffers == null) throw new ArgumentNullException();
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