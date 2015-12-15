namespace Netool.Network.DataFormats.StreamWrappers
{
    /// <summary>
    /// Generic stream wrapper - use it if you don't want/need to write a whole class for stream wrapper
    /// and only want to use a delegate.
    /// </summary>
    public class BasicWrapper : IStreamWrapper
    {
        public delegate IDataStream WrapperDelegate(IDataStream s);

        private WrapperDelegate wrapper;

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public string Params { get; private set; }

        public BasicWrapper(WrapperDelegate wrapper, string name, string wrapperParams)
        {
            Name = name;
            Params = wrapperParams;
            this.wrapper = wrapper;
        }

        public BasicWrapper(WrapperDelegate wrapper, string name, params object[] wrapperParams)
        {
            Name = name;
            this.wrapper = wrapper;
            this.Params = string.Join(", ", wrapperParams);
        }

        /// <inheritdoc/>
        public IDataStream Wrap(IDataStream s)
        {
            return wrapper(s);
        }
    }
}