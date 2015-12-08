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

        private struct ChunkHint
        {
            public long StreamStart;
            public long StreamLength;
            public long DataStart;
            public long DataLength;
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
                        var seg = new StreamSegment(stream);
                        long streamStart = 0, dataStart = 0;
                        while (seg.Length > 0)
                        {
                            var chunkInfo = ChunkedDecoder.DecodeOneChunk(seg);
                            var dl = chunkInfo.DataLength;
                            chunkHints.Add(new ChunkHint { StreamStart = streamStart, StreamLength = chunkInfo.ChunkLength, DataStart = dataStart, DataLength = dl });
                            streamStart += chunkInfo.ChunkLength;
                            dataStart += dl;
                            length += dl;
                            if (dl == 0)
                            {
                                return length;
                            }
                            seg = new StreamSegment(stream, seg.Offset + chunkInfo.ChunkLength, seg.Length - chunkInfo.ChunkLength);
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

        private ChunkHint locateChunk(long index)
        {
            if (index >= Length) throw new IndexOutOfRangeException();
            lock (dataLock)
            {
                ChunkHint hint;
                int i = 0;
                bool notFound = false;
                if (chunkHints.Count == 0)
                {
                    hint = new ChunkHint { StreamStart = 0, StreamLength = 0, DataStart = 0, DataLength = 0 };
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
                    var seg = new StreamSegment(stream, hint.StreamStart + hint.StreamLength);
                    var chunkInfo = ChunkedDecoder.DecodeOneChunk(seg);
                    hint = new ChunkHint { StreamStart = seg.Offset, StreamLength = chunkInfo.ChunkLength, DataStart = hint.DataStart + hint.DataLength, DataLength = chunkInfo.DataLength };
                    if (hint.DataLength == 0) throw new InvalidChunkException();
                    chunkHints.Add(hint);
                    i++;
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
                return hint;
            }
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            var hint = locateChunk(index);
            var seg = new StreamSegment(stream, hint.StreamStart);
            var chunkInfo = ChunkedDecoder.DecodeOneChunk(seg);
            return seg.ReadByte(chunkInfo.DataStart + index - hint.DataStart);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            var remaining = length;
            var hint = locateChunk(start);
            // these 2 must be close together, 2GiB chunks are too big
            int chunkOffset = (int)(start - hint.DataStart);
            var seg = new StreamSegment(stream, hint.StreamStart);
            while (remaining > 0)
            {
                var chunkInfo = ChunkedDecoder.DecodeOneChunk(seg);
                var read = Math.Min(chunkInfo.DataLength, remaining);
                seg.ReadBytesToBuffer(buffer, chunkInfo.DataStart + chunkOffset, read, offset);
                offset += read;
                remaining -= read;
                chunkOffset = 0;
                seg = new StreamSegment(stream, seg.Offset + chunkInfo.ChunkLength);
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