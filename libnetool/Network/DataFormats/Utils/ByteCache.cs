using System;
namespace Netool.Network.DataFormats.Utils
{
    /// <summary>
    /// Helper class for easily caching logical byte arrays
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ByteCache
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cacheStart">underlaying data index coresponding to buffer index 0</param>
        /// <param name="cacheLength">how many data were cached</param>
        public delegate void FillCache(byte[] buffer, out long cacheStart, out int cacheLength);

        public int MaxCacheSize { get; private set; }
        private byte[] buffer;

        private long start = 0;
        private int length = 0;

        /// <summary>
        /// Underlaying data index corresponding to buffer index 0
        /// </summary>
        public long Start { get { return start; } }

        /// <summary>
        /// Number of valid entries in cache
        /// </summary>
        public int Length { get { return length; } }

        public ByteCache(int maxCacheSize)
        {
            MaxCacheSize = maxCacheSize;
        }

        /// <summary>
        /// Try to read byte from cache
        /// </summary>
        /// <param name="index">absolute index (will be recalculated relatively to cacheStart)</param>
        /// <param name="res">result</param>
        /// <returns>false = cache miss (res = 0), true = cache hit</returns>
        public bool TryReadByte(long index, out byte res)
        {
            if (buffer == null || index >= Start + Length || index < Start)
            {
                res = 0;
                return false;
            }
            else
            {
                res = buffer[index - Start];
                return true;
            }
        }

        /// <summary>
        /// Tries to read all available data in given interval into the buffer
        /// </summary>
        /// <param name="buffer">output buffer</param>
        /// <param name="start">absolute index of first byte requested</param>
        /// <param name="length">how many to read</param>
        /// <param name="offset">start offset in the output buffer</param>
        /// <param name="firstRead">absolute index of first byte that was successfully read</param>
        /// <param name="read">how many bytes were read</param>
        /// <returns>true - at least one byte was read, false otherwise</returns>
        /// <remarks>
        /// If the first(F) requested byte is not found in cache but some other(O) is then it will be placed
        /// into the output buffer on the position (offset + O - F).
        /// </remarks>
        /// <exception cref="ArgumentNullException">buffer</exception>
        /// <exception cref="ArgumentOutOfRangeException">length</exception>
        public bool TryReadBytesToBuffer(byte[] buffer, long start, int length, int offset, out long firstRead, out int read)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length - offset < length) throw new ArgumentOutOfRangeException("length");
            if (this.buffer == null || start >= Start + Length || start  + length < Start)
            {
                firstRead = -1;
                read = 0;
                return false;
            }
            else
            {
                firstRead = Math.Max(start, Start);
                length -= (int)(firstRead - start);
                offset += (int)(firstRead - start);
                read = (int)Math.Min(length, Length - (firstRead - Start));
                Array.Copy(this.buffer, firstRead - Start, buffer, offset, read);
                return true;
            }
        }

        /// <summary>
        /// Shifts cache contents to left
        /// </summary>
        /// <param name="newCacheStart">new cache start - must be bigger or equal to cache.Start</param>
        /// <exception cref="ArgumentOutOfRangeException">newCacheStart is less than cache.Start</exception>
        public void ShiftLeft(long newCacheStart)
        {
            if (newCacheStart < start) throw new ArgumentOutOfRangeException("newCacheStart is less than cache.Start");
            if (buffer == null || newCacheStart == start) return;
            if (newCacheStart >= start + length)
            {
                length = 0;
                start = 0;
            }
            else
            {
                Array.Copy(buffer, newCacheStart - start, buffer, 0, length - (newCacheStart - start));
                length = length - (int)(newCacheStart - start);
                start = newCacheStart;
            }
        }

        /// <summary>
        /// Cache new data
        /// </summary>
        /// <param name="callback">caching callback to call</param>
        public void Cache(FillCache callback)
        {
            if (buffer == null) buffer = new byte[MaxCacheSize];
            callback(buffer, out start, out length);
        }

        /// <summary>
        /// Perform quick reset of cache
        /// </summary>
        /// <remarks>
        /// This method doesn't deallocate the buffer
        /// </remarks>
        public void Reset()
        {
            start = 0;
            length = 0;
        }
    }
}