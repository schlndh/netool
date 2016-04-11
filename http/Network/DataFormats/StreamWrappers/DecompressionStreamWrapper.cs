using System.IO;

namespace Netool.Network.DataFormats.StreamWrappers
{
    /// <summary>
    /// Common functionality wrapper for using standard decompression streams in stream wrappers
    /// </summary>
    internal class DecompressionStreamWrapper : BasicWrapper
    {
        private static BasicWrapper.WrapperDelegate delegateConverter(StreamFactoryDelegate streamFactory)
        {
            return delegate (IDataStream s)
            {
                var ret = new FromUnseekableStream(delegate () { return streamFactory(new ToStream(s)); });
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

        public DecompressionStreamWrapper(StreamFactoryDelegate d, string wrapperName, string wrapperParams) :
            base(delegateConverter(d), wrapperName, wrapperParams)
        { }

        public DecompressionStreamWrapper(StreamFactoryDelegate d, string wrapperName, params object[] wrapperParams) :
            base(delegateConverter(d), wrapperName, wrapperParams)
        { }
    }
}