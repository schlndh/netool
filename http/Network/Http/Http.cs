using Netool.Logging;

namespace Netool.Network.Http
{
    /// <summary>
    /// Basic interface for Http protocol upgrade (Upgrade header)
    /// </summary>
    /// <remarks>
    /// The inner channel is unlocked automatically by http channel after ChannelReplaced event is raised.
    /// Don't unlock the inner channel yourself.
    /// </remarks>
    public interface IProtocolUpgrader
    {
        IServerChannel UpgradeServerChannel(IServerChannel c, InstanceLogger logger);

        IClientChannel UpgradeClientChannel(IClientChannel c, InstanceLogger logger);
    }
}