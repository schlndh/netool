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
        private int chunkSize;
        private IDataStream innerStream;

        [NonSerialized]
        private StreamList tmpData = new StreamList();

        [NonSerialized]
        private long nextInnerData = 0;

        [NonSerialized]
        private object dataLock = new object();

        [NonSerialized]
        private IDataStream normalChunkStart;

        private long length = -1;

        /// <inheritdoc/>
        public long Length { get { if (length < 0) calculateLength(); return length; } }

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
            dataLock = new object();
            nextInnerData = 0;
            normalChunkStart = createChunkStart(chunkSize);
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            IDataStreamHelpers.ReadByteArgsCheck(this, index);
            prepareDataToIndex(index);
            lock (dataLock)
            {
                return tmpData.ReadByte(index);
            }
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            IDataStreamHelpers.ReadBytesToBufferArgsCheck(this, buffer, start, ref length, offset);
            prepareDataToIndex(start + length);
            lock (dataLock)
            {
                tmpData.ReadBytesToBuffer(buffer, start, length, offset);
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            var o = (IDataStream)innerStream.Clone();
            // test immutability
            if (object.ReferenceEquals(o, innerStream)) return this;
            return new ChunkedStream(o, chunkSize);
        }

        private void calculateLength()
        {
            lock (dataLock)
            {
                length = innerStream.Length;
                length += 5; // 0\r\n\r\n
                int chunkSizeLen = chunkSize.ToString("X").Length;
                length += (innerStream.Length / chunkSize) * (chunkSizeLen + 4); // XX\r\n...DATA...\r\n
                int lastChunkSize = (int)(innerStream.Length % chunkSize);
                if (lastChunkSize > 0)
                {
                    length += lastChunkSize.ToString("X").Length + 4;
                }
            }
        }

        private void prepareDataToIndex(long lastIndex)
        {
            lock (dataLock)
            {
                while (tmpData.Length <= lastIndex)
                {
                    int currChunkLen = Math.Min(chunkSize, (int)Math.Min(int.MaxValue, innerStream.Length - nextInnerData));
                    if (currChunkLen == 0)
                    {
                        tmpData.Add(streamEnd);
                        return;
                    }
                    if (currChunkLen == chunkSize) tmpData.Add(normalChunkStart);
                    else tmpData.Add(createChunkStart(currChunkLen));
                    tmpData.Add(new StreamSegment(innerStream, nextInnerData, currChunkLen));
                    tmpData.Add(chunkEnd);
                    nextInnerData += currChunkLen;
                }
            }
        }

        private IDataStream createChunkStart(int chunkLength)
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