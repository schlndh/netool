using Netool.Logging;
using System;
using System.Runtime.Serialization;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Data stream for reading a logged file
    /// </summary>
    /// <remarks>
    /// Use this stream type when logging huge data amounts (eg. a large file transported over HTTP).
    /// When using this data type only a lightweight wrapper will be deserialized at first and data will then be loaded as requested.
    /// </remarks>
    [Serializable]
    public class LoggedFile : IDataStream
    {
        public long Length
        {
            get
            {
                if(length < 0)
                {
                    var reader = log.CreateReader();
                    length = reader.GetFileLength(hint);
                }
                return length;
            }
        }
        private long id;
        [NonSerialized]
        private long hint;
        [NonSerialized]
        private long length = -1;
        [NonSerialized]
        private FileLog log;
        [NonSerialized]
        private byte[] byteCache;
        [NonSerialized]
        private long cacheStart = 0;
        [NonSerialized]
        private int cacheLength = 0;

        public LoggedFile(long ID, FileLog log)
        {
            this.id = ID;
            this.log = log;
            init();
        }

        /// <inheritdoc />
        /// <remarks>Assumes nearly sequential access.</remarks>
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            if(byteCache == null || index < cacheStart || index >= cacheStart + cacheLength)
            {

                if (byteCache == null) byteCache = new byte[256];
                var reader = log.CreateReader();
                // the file is more likely to be read from the beginning to the end
                cacheStart = Math.Max(0, index - 32);
                cacheLength = (int)Math.Min(256, length - index + 64);
                reader.ReadFileDataToBuffer(hint, byteCache, cacheStart, cacheLength, 0);
                reader.Close();
            }
            return byteCache[index - cacheStart];
        }

        /// <inheritdoc />
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            var reader = log.CreateReader();
            reader.ReadFileDataToBuffer(hint, buffer, start, length, offset);
            reader.Close();
        }

        /// <inheritdoc />
        public object Clone()
        {
            // this object is not directly mutable
            return this;
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            var c = context.Context as FileLogReader.DeserializationContext;
            if(c == null)
            {
                throw new InvalidContextException();
            }
            this.log = c.Log;
            this.length = -1;
            init();
        }

        private void init()
        {
            var reader = log.CreateReader();
            hint = reader.GetFileHint(id);
        }
    }
}
