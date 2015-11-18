namespace Netool.Network.DataFormats.Utils
{
    /// <summary>
    /// Helper class for easily caching logical byte arrays
    /// </summary>
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

        private long start;
        private int length;

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
        /// Cache new data
        /// </summary>
        /// <param name="callback">caching callback to call</param>
        public void Cache(FillCache callback)
        {
            if (buffer == null) buffer = new byte[MaxCacheSize];
            callback(buffer, out start, out length);
        }
    }
}