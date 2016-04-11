using Netool.Network.DataFormats;
using Netool.Network.DataFormats.StreamWrappers;
using Netool.Plugins.Http;
using System;
using System.IO;
using System.IO.Compression;

namespace Netool.Plugins.StreamWrappers
{
    public class UnDeflateStreamWrapperPlugin : IStreamDecoderPlugin
    {
        /// <inheritdoc />
        public long ID { get { return 4007; } }

        /// <inheritdoc />
        public string Name { get { return "UnDeflateStreamWrapperPlugin"; } }

        /// <inheritdoc />
        public string Description { get { return "Plugin for UnDeflate Stream wrapper"; } }

        /// <inheritdoc />
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc />
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc />
        public string EncodingName { get { return "deflate"; } }

        /// <inheritdoc />
        public Network.DataFormats.IStreamWrapper CreateWrapper()
        {
            return new DecompressionStreamWrapper(delegate (Stream s)
            {
                return new DeflateStream(s, CompressionMode.Decompress);
            },
            "undeflate");
        }
    }
}