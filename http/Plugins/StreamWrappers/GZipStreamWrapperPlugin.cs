using Netool.Network.DataFormats;
using Netool.Network.DataFormats.StreamWrappers;
using System;
using System.IO;
using System.IO.Compression;

namespace Netool.Plugins.StreamWrappers
{
    public class GZipStreamWrapperPlugin : IStreamWrapperPlugin
    {
        /// <inheritdoc />
        public long ID { get { return 4004; } }

        /// <inheritdoc />
        public string Name { get { return "GZipStreamWrapperPlugin"; } }

        /// <inheritdoc />
        public string Description { get { return "Plugin for GZip Stream wrapper"; } }

        /// <inheritdoc />
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc />
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc />
        public Network.DataFormats.IStreamWrapper CreateWrapper()
        {
            return new CompressionStreamWrapper(delegate (Stream s)
            {
                return new GZipStream(s, CompressionMode.Compress, true);
            },
            "gzip", "default");
        }
    }
}