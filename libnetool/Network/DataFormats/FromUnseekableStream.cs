using Netool.Network.DataFormats.Utils;
using System;
using System.IO;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// A wrapper for unseekable standard stream into a IDataStream.
    /// </summary>
    /// <remarks>
    /// Use FromStream for seekable streams (such as FileStream) as it is more efficient.
    /// </remarks>
    public class FromUnseekableStream : IDataStream
    {
        public delegate Stream StreamFactory();

        // the last byte in cache will always be streamPosition-1
        private ByteCache cache;

        private long length = -1;
        private StreamFactory factory;
        private Stream stream;
        private long streamPosition = 0;
        private object dataLock = new object();
        private byte[] seekBuffer = new byte[4096];

        public FromUnseekableStream(StreamFactory factory, int cacheSize = 5*1024*1024)
        {
            cache = new ByteCache(cacheSize);
            this.factory = factory;
        }

        /// <inheritdoc/>
        public long Length { get { if (length == -1) calculateLength(); return length; } }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            byte ret;
            lock (dataLock)
            {
                if (cache.TryReadByte(index, out ret)) return ret;
                seekToPosition(index);
                cache.TryReadByte(index, out ret);
                return ret;
            }
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            lock (dataLock)
            {
                while (length > 0)
                {
                    int chunk = Math.Min(length, cache.MaxCacheSize);
                    seekToPosition(start + chunk - 1);
                    // no need to check these, because the cache is set so that it will be ok
                    long firstRead;
                    int read;
                    cache.TryReadBytesToBuffer(buffer, start, chunk, offset, out firstRead, out read);
                    length -= chunk;
                    start += chunk;
                    offset += chunk;
                }
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }

        private void calculateLength()
        {
            lock (dataLock)
            {
                if (stream == null) stream = factory();
                else if (streamPosition > 0)
                {
                    stream.Dispose();
                    stream = factory();
                    streamPosition = 0;
                }
                length = 0;
                int read = 0;
                while ((read = stream.Read(seekBuffer, 0, seekBuffer.Length)) != 0)
                {
                    length += read;
                }
                stream.Dispose();
                stream = null;
            }
        }

        private void seekToPosition(long position)
        {
            if (stream == null) stream = factory();
            if (position >= streamPosition)
            {
                long newCacheStart = Math.Max(0, position - cache.MaxCacheSize + 1);
                cache.ShiftLeft(newCacheStart);
                seekHelper(position);
            }
            else
            {
                if (position > cache.Start && position < cache.Start + cache.Length)
                {
                    return;
                }
                else
                {
                    streamPosition = 0;
                    stream.Dispose();
                    stream = factory();
                    cache.Reset();
                    seekHelper(position);
                }
            }
        }

        private void seekHelper(long position)
        {
            long newCacheStart = Math.Max(0, position - cache.MaxCacheSize + 1);
            int read = 0;
            while ((read = stream.Read(seekBuffer, 0, (int)Math.Min(seekBuffer.Length, position - streamPosition + 1))) != 0)
            {
                if (streamPosition + read >= newCacheStart)
                    cache.Cache(delegate(byte[] buffer, out long cacheStart, out int cacheLength)
                    {
                        int newlyCached = read - (int)(Math.Max(0, newCacheStart - streamPosition));
                        Array.Copy(seekBuffer, Math.Max(newCacheStart - streamPosition, 0), buffer, cache.Length, newlyCached);
                        cacheStart = newCacheStart;
                        cacheLength = cache.Length + newlyCached;
                    });
                streamPosition += read;
            }
        }
    }
}