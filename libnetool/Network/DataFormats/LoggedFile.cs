using Netool.Logging;
using Netool.Network.DataFormats.Utils;
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
                    reader.Close();
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
        private ByteCache cache = new ByteCache(256);

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
            byte ret;
            if(!cache.TryReadByte(index, out ret))
            {
                ByteCache.FillCache callback = delegate(byte[] buffer, out long cacheStart, out int cacheLength)
                {
                    var reader = log.CreateReader();
                    // the file is more likely to be read from the beginning to the end
                    cacheStart = Math.Max(0, index - 32);
                    cacheLength = (int)Math.Min(buffer.Length, Math.Min(int.MaxValue, length - cacheStart));
                    reader.ReadFileDataToBuffer(hint, buffer, cacheStart, cacheLength, 0);
                    ret = buffer[index - cacheStart];
                    reader.Close();
                };
                cache.Cache(callback);
            }
            return ret;
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
            reader.Close();
        }
    }
}
