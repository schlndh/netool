using Netool.Logging;

namespace Netool.Network.Http
{
    /// <summary>
    /// Encapsulates given channel into a new channel.
    /// </summary>
    /// <remarks>
    /// Exact channel type given is not defined and cannot be relied upon.
    /// </remarks>
    public interface IProtocolUpgrader
    {
        IServerChannel UpgradeServerChannel(IServerChannel c, InstanceLogger logger);

        IClientChannel UpgradeClientChannel(IClientChannel c, InstanceLogger logger);
    }
}