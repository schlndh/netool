using System;
using System.Runtime.Serialization;
using System.Text;

namespace Netool.Network.DataFormats.Http
{
    /// <summary>
    /// Stream that chunks given inner stream into chunks of regular size
    /// </summary>
    [Serializable]
    public class ChunkedStream : IDataStream
    {
        private class RegularyChunkedStream : IDataStream
        {
            private long length;
            private long chunkCount;
            private int chunkSize;
            private int fullChunkSize;
            private IDataStream innerStream;
            private IDataStream chunkStart;
            public long Length { get { return length; } }

            public RegularyChunkedStream(IDataStream innerStream, int chunkSize, long chunkCount)
            {
                this.innerStream = innerStream;
                this.chunkSize = chunkSize;
                this.chunkCount = chunkCount;
                this.fullChunkSize = chunkSize.ToString("X").Length + 4 + chunkSize;
                this.length = chunkCount * fullChunkSize;
                chunkStart = createChunkStart(chunkSize);
            }

            public byte ReadByte(long index)
            {
                var off = index % fullChunkSize;
                if(off < chunkStart.Length)
                {
                    return chunkStart.ReadByte(off);
                }
                else if(off > fullChunkSize - 3)
                {
                    return chunkEnd.ReadByte(off - (fullChunkSize - 2));
                }
                else
                {
                    var chunk = index / fullChunkSize;
                    off -= chunkStart.Length;
                    return innerStream.ReadByte(chunk * chunkSize + off);
                }
            }

            public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
            {
                IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
                while(length > 0)
                {
                    var off = start % fullChunkSize;
                    int read;
                    if (off < chunkStart.Length)
                    {
                        read = (int)Math.Min(length, chunkStart.Length - off);
                        chunkStart.ReadBytesToBuffer(buffer, off, read, offset);
                    }
                    else if (off > fullChunkSize - 3)
                    {
                        read = (int)Math.Min(length, fullChunkSize - off);
                        chunkEnd.ReadBytesToBuffer(buffer, off - (fullChunkSize - 2), read, offset);
                    }
                    else
                    {
                        var chunk = start / fullChunkSize;
                        off -= chunkStart.Length;
                        read = (int)Math.Min(length, chunkSize - off);
                        innerStream.ReadBytesToBuffer(buffer, chunk * chunkSize + off, read, offset);
                    }
                    length -= read;
                    offset += read;
                    start += read;
                }
            }

            public object Clone()
            {
                return this;
            }
        }
        private int chunkSize;
        private IDataStream innerStream;

        [NonSerialized]
        private StreamList tmpData;

        /// <inheritdoc/>
        public long Length { get { return tmpData.Length; } }

        private static IDataStream chunkEnd = new ByteArray(new byte[] { 13, 10 }); // \r\n
        private static IDataStream streamEnd = new ByteArray(new byte[] { 48, 13, 10, 13, 10 }); //0\r\n\r\n

        public ChunkedStream(IDataStream s, int chunkSize)
        {
            if (s == null) throw new ArgumentNullException("s");
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException("chunkSize must be positive");
            this.innerStream = s;
            this.chunkSize = chunkSize;
            init();
        }

        private void init()
        {
            tmpData = new StreamList();
            var chunks = innerStream.Length / chunkSize;
            if (chunks > 0) tmpData.Add(new RegularyChunkedStream(innerStream, chunkSize, chunks));
            var lastChunkLength = (int)(innerStream.Length % chunkSize);
            if (lastChunkLength > 0)
            {
                tmpData.Add(createChunkStart(lastChunkLength));
                tmpData.Add(new StreamSegment(innerStream, chunkSize * (innerStream.Length / chunkSize)));
                tmpData.Add(chunkEnd);
            }
            tmpData.Add(streamEnd);
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            return tmpData.ReadByte(index);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            tmpData.ReadBytesToBuffer(buffer, start, length, offset);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            var o = (IDataStream)innerStream.Clone();
            // test immutability
            if (object.ReferenceEquals(o, innerStream)) return this;
            return new ChunkedStream(o, chunkSize);
        }

        private static IDataStream createChunkStart(int chunkLength)
        {
            return new ByteArray(ASCIIEncoding.ASCII.GetBytes(chunkLength.ToString("X") + "\r\n"));
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            init();
        }
    }
}