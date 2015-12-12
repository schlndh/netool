using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netool.Network.DataFormats.Http
{
    /// <summary>
    /// Decodes chunked encoding on-the-fly and wraps the resulting data in IDataStream.
    /// </summary>
    /// <remarks>
    /// This class does NOT work with mutable streams.
    /// </remarks>
    [Serializable]
    public class DechunkedStream : IDataStream
    {
        private IDataStream stream;
        private long length;
        [NonSerialized]
        private List<ChunkHint> chunkHints = new List<ChunkHint>();
        [NonSerialized]
        private object dataLock = new object();

        private class ChunkHint
        {
            /// <summary>
            /// Start of chunk data in the stream (excluding the chunk header)
            /// </summary>
            public long StreamStart;

            /// <summary>
            /// Stream position of next chunk in the stream
            /// </summary>
            public long NextChunk;

            /// <summary>
            /// Index of the first byte of the chunk in dechunked stream
            /// </summary>
            public long DataStart;

            /// <summary>
            /// Length of data in this chunk
            /// </summary>
            public long DataLength;

            public ChunkHint ComputeNextHint(DechunkedStream stream)
            {
                var seg = new StreamSegment(stream.stream, NextChunk);
                var chunkInfo = ChunkedDecoder.DecodeOneChunk(seg);
                // last chunk
                if (chunkInfo.DataLength == 0) return null;
                else return new ChunkHint { StreamStart = this.NextChunk + chunkInfo.DataStart, NextChunk = this.NextChunk + chunkInfo.ChunkLength, DataLength = chunkInfo.DataLength, DataStart = this.DataStart + this.DataLength };
            }
        }

        /// <inheritdoc/>
        public long Length
        {
            get
            {
                lock (dataLock)
                {
                    if (length == -1)
                    {
                        length = 0;
                        if (stream.Length == 0) return length;
                        var hint = new ChunkHint();
                        while((hint = hint.ComputeNextHint(this)) != null)
                        {
                            length += hint.DataLength;
                            chunkHints.Add(hint);
                        }
                    }
                    return length;
                }
            }
        }

        public DechunkedStream(IDataStream stream, long length = -1)
        {
            this.stream = stream;
            this.length = length;
        }

        private int locateChunk(long index)
        {
            if (index >= Length) throw new IndexOutOfRangeException();

                ChunkHint hint;
                int i = 0;
                bool notFound = false;
                if (chunkHints.Count == 0)
                {
                    hint = new ChunkHint { StreamStart = 0, NextChunk = 0, DataStart = 0, DataLength = 0 };
                    notFound = true;
                }
                else
                {
                    // it is likely that the stream will be read sequentially from start to end
                    i = chunkHints.Count - 1;
                    hint = chunkHints[i];
                }
                while (notFound || hint.DataStart + hint.DataLength <= index)
                {
                    notFound = false;
                    hint = hint.ComputeNextHint(this);
                    chunkHints.Add(hint);
                    i = chunkHints.Count - 1;
                }
                int interval = chunkHints.Count / 2;
                while (hint.DataStart > index || index >= hint.DataStart + hint.DataLength)
                {
                    if (hint.DataStart > index)
                    {
                        i -= interval;
                    }
                    else
                    {
                        i += interval;
                    }
                    interval = Math.Max(interval/2, 1);
                    hint = chunkHints[i];
                }
                return i;
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            ChunkHint hint;
            lock (dataLock)
            {
                var hintIdx = locateChunk(index);
                hint = chunkHints[hintIdx];
            }
            var seg = new StreamSegment(stream, hint.StreamStart);
            return seg.ReadByte(index - hint.DataStart);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            var remaining = length;
            var hintIdx = locateChunk(start);
            var hint = chunkHints[hintIdx];
            // these 2 must be close together, 2GiB chunks are too big
            int chunkOffset = (int)(start - hint.DataStart);
            while (remaining > 0)
            {
                lock (dataLock)
                {
                    if (chunkHints.Count <= hintIdx)
                    {
                        hint = hint.ComputeNextHint(this);
                        chunkHints.Add(hint);
                    }
                    else
                    {
                        hint = chunkHints[hintIdx];
                    }
                }
                var seg = new StreamSegment(stream, hint.StreamStart);
                var read = (int)Math.Min(hint.DataLength - chunkOffset, remaining);
                seg.ReadBytesToBuffer(buffer, chunkOffset, read, offset);
                offset += read;
                remaining -= read;
                chunkOffset = 0;
                ++hintIdx;
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            IDataStream clone = (IDataStream)stream.Clone();
            // DechunkedStream is immutable unless underlying stream is mutable
            if (object.ReferenceEquals(stream, clone)) return this;
            return new DechunkedStream(clone, Length);
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            chunkHints = new List<ChunkHint>();
            dataLock = new object();
        }
    }
}