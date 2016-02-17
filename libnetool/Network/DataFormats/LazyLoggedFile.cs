using Netool.Logging;
using System;
using System.Runtime.Serialization;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Data stream that writes underlaying stream into a LoggedFile upon serialization
    /// </summary>
    [Serializable]
    public class LazyLoggedFile : IDataStream
    {
        /// <inheritdoc/>
        public long Length { get { return innerStream.Length; } }

        private IDataStream innerStream;

        public LazyLoggedFile(IDataStream stream)
        {
            innerStream = stream;
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            return innerStream.ReadByte(index);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            innerStream.ReadBytesToBuffer(buffer, start, length, offset);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            var tmp = (IDataStream)innerStream.Clone();
            // test immutability
            if (object.ReferenceEquals(tmp, innerStream)) return this;
            return new LazyLoggedFile(tmp);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            var log = (context.Context as FileLog.SerializationContext).Log;
            if (!(innerStream is LoggedFile))
            {
                var builder = new LoggedFileBuilder(log);
                builder.Append(innerStream);
                innerStream = builder.Close();
            }
        }
    }
}