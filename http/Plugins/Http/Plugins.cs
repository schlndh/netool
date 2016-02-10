using Netool.Network;
using Netool.Network.Helpers;
using Netool.Network.Http;
using Netool.Network.DataFormats.Http;
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

    /// <summary>
    /// Basic interface for creating IProtocolUpgrader
    /// </summary>
    /// <remarks>
    /// While it's not part of the interface it is a good practice to create a parametrized overload for
    /// CreateUpgrader method, so that it can be used from channel drivers.
    /// </remarks>
    public interface IProtocolUpgradePlugin : IPlugin
    {
        string ProtocolName { get; }

        /// <summary>
        /// Create a protocol upgrader
        /// </summary>
        /// <remarks>
        /// This method can ask for user input - eg. display a dialog window.
        /// </remarks>
        /// <exception cref="SetupAbortedByUserException"></exception>
        /// <returns></returns>
        IProtocolUpgrader CreateUpgrader();
    }
}