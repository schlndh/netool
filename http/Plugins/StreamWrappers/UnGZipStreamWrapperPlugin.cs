using Netool.Network.DataFormats;
using Netool.Network.DataFormats.StreamWrappers;
using Netool.Plugins.Http;
using System;
using System.IO.Compression;

namespace Netool.Plugins.StreamWrappers
{
    public class UnGZipStreamWrapperPlugin : IStreamDecoderPlugin
    {
        /// <inheritdoc />
        public long ID { get { return 4005; } }

        /// <inheritdoc />
        public string Name { get { return "UnGZipStreamWrapperPlugin"; } }

        /// <inheritdoc />
        public string Description { get { return "Plugin for UnGZip Stream wrapper"; } }

        /// <inheritdoc />
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc />
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc />
        public string EncodingName { get { return "gzip"; } }

        /// <inheritdoc />
        public Network.DataFormats.IStreamWrapper CreateWrapper()
        {
            return new BasicWrapper(delegate(IDataStream s)
            {
                var ret = new FromUnseekableStream(delegate() { return new GZipStream(new ToStream(s), CompressionMode.Decompress); });
                if (ret.Length > 5 * 1024 * 1024)
                {
                    return new LazyLoggedFile(ret);
                }
                else
                {
                    return new ByteArray(ret);
                }
            }, "ungzip");
        }
    }
}