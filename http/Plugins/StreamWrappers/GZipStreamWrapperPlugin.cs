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
            return new BasicWrapper(delegate(IDataStream s)
            {
                Stream baseStream;
                if (s.Length > 50 * 1024 * 1024)
                {
                    baseStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                }
                else
                {
                    baseStream = new MemoryStream();
                }
                using (var gzip = new GZipStream(baseStream, CompressionMode.Compress, true))
                {
                    var tmpStream = new ToStream(s);
                    tmpStream.CopyTo(gzip);
                }

                var ret = FromStream.ToIDataStream(baseStream);
                if (ret.Length > 5 * 1024 * 1024)
                {
                    return new LazyLoggedFile(ret);
                }
                else
                {
                    return new ByteArray(ret);
                }
            }, "gzip", "default");
        }
    }
}