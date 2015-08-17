using System;
using System.IO;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Wrapper class to make standard stream into an IDataStream
    /// </summary>
    public class FromStream : IDataStream
    {
        private Stream stream;
        /// <inheritdoc/>
        public long Length { get { return stream.Length; } }

        /// <summary>
        /// Wraps standard stream into an IDataStream. Given stream must support reading and seeking.
        /// </summary>
        /// <param name="stream"></param>
        private FromStream(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Converts standard Stream to IDataStream.
        /// </summary>
        /// <param name="stream">Stream to convert. Must be readable.</param>
        /// <returns></returns>
        /// <remarks>
        /// Note that if stream doesn't support seeking then it will be read entirely to memory.
        /// </remarks>
        public static IDataStream ToIDataStream(Stream stream)
        {
            if (!stream.CanRead) throw new ArgumentException("Stream must be readable!");
            if(stream.CanSeek)
            {
                return new FromStream(stream);
            }
            else
            {
                var list = new StreamList();
                int bytesRead = 0;
                var buffer = new byte[4096];
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        list.Add(new ByteArray(buffer, 0, bytesRead));
                    }
                } while (bytesRead > 0);
                return list;
            }
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            if (index < 0 || index >= stream.Length) throw new ArgumentOutOfRangeException();
            stream.Position = index;
            return (byte) stream.ReadByte();
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(System.Collections.Generic.IList<ArraySegment<byte>> buffers, long start = 0, long length = -1)
        {
            if (start < 0 || start >= stream.Length) throw new ArgumentOutOfRangeException();
            if(length == -1)
            {
                length = stream.Length - start;
            }
            stream.Position = start;
            long read = 0;
            foreach(var buff in buffers)
            {
                int len = Math.Max((int) Math.Min(length - read, int.MaxValue), buff.Count);
                stream.Read(buff.Array, buff.Offset, len);
                read += len;
                if (length == read) break;
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            // can't clone a stream anyway
            return this;
        }
    }
}
