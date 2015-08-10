using System;
using System.Collections.Generic;
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
        private ReaderWriterLockSlim streamsLock = new ReaderWriterLockSlim();
        /// <inheritdoc/>
        public long Length { get { return Interlocked.Read(ref length); } }

        /// <summary>
        /// Add stream to the end of the list
        /// </summary>
        /// <param name="s">stream</param>
        public void Add(IDataStream s)
        {
            streamsLock.EnterWriteLock();
            try
            {
                streams.Add((IDataStream) s.Clone());
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
                if(index < 0 || index >= length)
                {
                    throw new ArgumentOutOfRangeException();
                }
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
        public void ReadBytesToBuffer(long start, long length, IList<ArraySegment<byte>> buffers)
        {
            if (length == 0) return;
            var workBuffers = new List<ArraySegment<byte>>(buffers);
            streamsLock.EnterReadLock();
            try
            {
                if (start < 0 || start + length > this.length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                int i = 0;
                var stream = streams[0];
                while (start >= stream.Length)
                {
                    if (i == streams.Count) throw new ArgumentOutOfRangeException();
                    start -= stream.Length;
                    stream = streams[++i];
                }

                do
                {
                    var len = Math.Min(length, stream.Length - start);
                    int skip = 0;
                    long off = 0;
                    length -= len;
                    stream.ReadBytesToBuffer(start, len, workBuffers);
                    foreach(var buff in workBuffers)
                    {
                        len -= buff.Count;
                        if(len < 0)
                        {
                            off = buff.Count  + len;
                            break;
                        }
                        ++skip;
                    }

                    len = 0;

                    if (skip > 0)
                    {
                        workBuffers = workBuffers.GetRange(skip, workBuffers.Count - skip);
                    }
                    if (off > 0)
                    {
                        workBuffers[0] = new ArraySegment<byte>(workBuffers[0].Array, (int)(workBuffers[0].Offset + off), (int)(workBuffers[0].Count - off));
                    }
                    i++;
                    if (i == streams.Count && length > 0) throw new ArgumentOutOfRangeException();
                    if (i == streams.Count) return;
                    stream = streams[i];
                    start = 0;
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
    }
}