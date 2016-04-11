using System.IO;

namespace Netool.Network.DataFormats.StreamWrappers
{
    /// <summary>
    /// Common functionality wrapper for using standard compression streams in stream wrappers
    /// </summary>
    internal class CompressionStreamWrapper : BasicWrapper
    {
        private static BasicWrapper.WrapperDelegate delegateConverter(StreamFactoryDelegate streamFactory)
        {
            return delegate (IDataStream s)
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
                using (var compression = streamFactory(baseStream))
                {
                    var tmpStream = new ToStream(s);
                    tmpStream.CopyTo(compression);
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
            };
        }

        public delegate Stream StreamFactoryDelegate(Stream s);

        public CompressionStreamWrapper(StreamFactoryDelegate d, string wrapperName, string wrapperParams) :
            base(delegateConverter(d), wrapperName, wrapperParams)
        { }

        public CompressionStreamWrapper(StreamFactoryDelegate d, string wrapperName, params object[] wrapperParams) :
            base(delegateConverter(d), wrapperName, wrapperParams)
        { }
    }
}