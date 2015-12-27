namespace Netool.Plugins.Http
{
    /// <summary>
    /// Interface for http stream decoders
    /// </summary>
    /// <remarks>
    /// Note that plugins implementing this interface are expected to
    /// return wrapper from CreateWrapper() without user interaction.
    /// </remarks>
    public interface IStreamDecoderPlugin : IStreamWrapperPlugin
    {
        /// <summary>
        /// Case-insensitive name of the encoding, as used in http Transfer-Encoding header, eg. chunked
        /// </summary>
        string EncodingName { get; }
    }
}