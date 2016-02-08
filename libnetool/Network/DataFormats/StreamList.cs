using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Mutable DataFormat for list of streams
    /// </summary>
    [Serializable]
    public class StreamList : IDataStream
    {
        private List<IDataStream> streams = new List<IDataStream>();
        private long length;
        [NonSerialized]
        private ReaderWriterLockSlim streamsLock = new ReaderWriterLockSlim();
        /// <inheritdoc/>
        public long Length { get { return Interlocked.Read(ref length); } }

        /// <summary>
        /// Add stream to the end of the list
        /// </summary>
        /// <param name="s">stream</param>
        public void Add(IDataStream s)
        {
            s = (IDataStream)s.Clone();
            streamsLock.EnterWriteLock();
            try
            {
                streams.Add(s);
                Interlocked.Add(ref length, s.Length);
            }
            finally
            {
                streamsLock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            streamsLock.EnterReadLock();
            try
            {
                IDataStreamHelpers.ReadByteArgsCheck(this, index);
                int i = 0;
                while(index >= 0)
                {
                    var stream = streams[i++];
                    if(index >= stream.Length)
                    {
                        index -= stream.Length;
                    }
                    else
                    {
                        return stream.ReadByte(index);
                    }
                }
                // unreachable code
                throw new ArgumentOutOfRangeException();
            }
            finally
            {
                streamsLock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            streamsLock.EnterReadLock();
            try
            {
                IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
                if (length == 0) return;
                var workBuffer = new ArraySegment<byte>(buffer, offset, length);
                int i = 0;
                var stream = streams[0];
                // move to the stream containing the start
                while (start >= stream.Length)
                {
                    if (i == streams.Count) throw new ArgumentOutOfRangeException();
                    start -= stream.Length;
                    stream = streams[++i];
                }

                do
                {
                    var len = (int)Math.Min(length, stream.Length - start);
                    length -= len;
                    stream.ReadBytesToBuffer(workBuffer.Array, start, len, workBuffer.Offset);
                    if(length > 0)
                    {
                        workBuffer = new ArraySegment<byte>(workBuffer.Array, workBuffer.Offset + len, workBuffer.Count - len);
                        i++;
                        if (i == streams.Count && length > 0) throw new ArgumentOutOfRangeException();
                        if (i == streams.Count) return;
                        stream = streams[i];
                        start = 0;
                    }
                } while (length > 0);
            }
            finally
            {
                streamsLock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            streamsLock.EnterReadLock();
            try
            {
                var newStreams = new List<IDataStream>(streams.Count);
                foreach(var stream in streams)
                {
                    newStreams.Add((IDataStream) stream.Clone());
                }
                return new StreamList{length = this.length, streams = newStreams};
            }
            finally
            {
                streamsLock.ExitReadLock();
            }
        }

        [OnDeserializing]
        private void SetValuesOnDeserializing(StreamingContext context)
        {
            streamsLock = new ReaderWriterLockSlim();
        }
    }
}