using Netool.Network;

namespace Netool.ChannelDrivers
{
    public interface IChannelDriver
    {
        /// <summary>
        /// Indicates whether the driver is capable of handling another channel
        /// </summary>
        bool CanAccept(IChannel c);
        /// <summary>
        /// This field is only used when new channel view is being created
        /// </summary>
        bool AllowManualControl { get; }
        string ID { get; }
    }

    public interface IServerChannelDriver : IChannelDriver
    {
        void Handle(IServerChannel c);
    }

    public interface IClientChannelDriver : IChannelDriver
    {
        void Handle(IClientChannel c);
    }

    public interface IProxyChannelDriver : IChannelDriver
    {
        void Handle(IProxyChannel c);
    }
}