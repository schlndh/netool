using Netool.Logging;
using Netool.Network.DataFormats.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

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
    public class LoggedFile : IDataStream, IEquatable<LoggedFile>
    {
        public long Length
        {
            get
            {
                if(length < 0)
                {
                    using (var reader = log.ReaderPool.Get())
                    {
                        length = reader.GetFileLength(hint);
                    }
                }
                return length;
            }
        }
        private long id;
        [NonSerialized]
        private FileLog.LoggedFileInfo hint;
        [NonSerialized]
        private long length = -1;
        [NonSerialized]
        private FileLog log;
        [NonSerialized]
        private ByteCache cache;
        [NonSerialized]
        private Dictionary<FileLog, long> otherLogFiles;
        [NonSerialized]
        private object cacheLock;
        [NonSerialized]
        private object serializationLock;
        [NonSerialized]
        private long backupId;

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
            lock(cacheLock)
            {
                if (!cache.TryReadByte(index, out ret))
                {
                    ByteCache.FillCache callback = delegate (byte[] buffer, out long cacheStart, out int cacheLength)
                    {
                        using (var reader = log.ReaderPool.Get())
                        {
                            // the file is more likely to be read from the beginning to the end
                            cacheStart = Math.Max(0, index - 32);
                            cacheLength = (int)Math.Min(buffer.Length, Math.Min(int.MaxValue, length - cacheStart));
                            reader.ReadFileDataToBuffer(hint, buffer, cacheStart, cacheLength, 0);
                            ret = buffer[index - cacheStart];
                        }
                    };
                    cache.Cache(callback);
                }
            }
            return ret;
        }

        /// <inheritdoc />
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            using (var reader = log.ReaderPool.Get())
            {
                reader.ReadFileDataToBuffer(hint, buffer, start, length, offset);
            }
        }

        /// <inheritdoc />
        public object Clone()
        {
            // this object is not directly mutable
            return this;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            Monitor.Enter(serializationLock);
            backupId = id;
            var log = (context.Context as FileLog.SerializationContext).Log;
            // serializing to another log file?
            if (!object.ReferenceEquals(this.log, log))
            {
                if(!otherLogFiles.TryGetValue(log, out id))
                {
                    var info = log.CreateFile();
                    otherLogFiles[log] = id = info.ID;
                    log.AppendDataToFile(info, this);
                }
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            id = backupId;
            Monitor.Exit(serializationLock);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
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
            using (var reader = log.ReaderPool.Get())
            {
                hint = reader.GetFileHint(id);
            }
            cache = new ByteCache(256);
            cacheLock = new object();
            serializationLock = new object();
            otherLogFiles = new Dictionary<FileLog, long>();
        }

        /// <inheritdoc/>
        /// <remarks>Overriden for testing purposes</remarks>
        bool IEquatable<LoggedFile>.Equals(LoggedFile other)
        {
            return log.Equals(other.log) && id == other.id;
        }
    }
}
