using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.DataFormats.StreamWrappers;
using Netool.Plugins.Http;
using System;

namespace Netool.Plugins.StreamWrappers
{
    public class DechunkedStreamWrapperPlugin : IStreamDecoderPlugin
    {
        /// <inheritdoc />
        public long ID { get { return 4001; } }

        /// <inheritdoc />
        public string Name { get { return "DechunkedStreamWrapperPlugin"; } }

        /// <inheritdoc />
        public string Description { get { return "Plugin for Dechunked Stream wrapper"; } }

        /// <inheritdoc />
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc />
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc />
        public string EncodingName { get { return "chunked"; } }

        /// <inheritdoc />
        public Network.DataFormats.IStreamWrapper CreateWrapper()
        {
            return new BasicWrapper(delegate(IDataStream s) { return new DechunkedStream(s); }, "dechunk", "");
        }
    }
}