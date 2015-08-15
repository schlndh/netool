using System;
using System.Collections.Generic;
using System.IO;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// Wrapper class to make any Netool's IDataStream into a standard stream.
    /// </summary>
    [Serializable]
    public class StreamWrapper : Stream
    {
        private IDataStream stream;
        private long position = 0;

        /// <inheritdoc/>
        public override bool CanWrite { get { return false; } }
        /// <inheritdoc/>
        public override bool CanRead { get { return true; } }
        /// <inheritdoc/>
        public override bool CanSeek { get { return true; } }
        /// <inheritdoc/>
        public override long Length { get { return stream.Length; } }
        /// <inheritdoc/>
        public override long Position { get { return position; } set { position = value; } }


        public StreamWrapper(IDataStream stream)
        {
            this.stream = stream;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int len = Math.Min(count, (int)(Math.Min(int.MaxValue, stream.Length - (position + offset))));
            stream.ReadBytesToBuffer(new List<ArraySegment<byte>> { new ArraySegment<byte>(buffer) }, position + offset, len);
            position += len;
            return len;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.End:
                    position = stream.Length + offset;
                    break;
            }
            return position;
        }

        /// <exclude />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <exclude />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <exclude />
        public override void Flush()
        {
            throw new NotSupportedException();
        }
    }
}
