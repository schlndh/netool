using Netool.Network;

namespace Netool.ChannelDrivers
{
    /// <summary>
    /// Channel driver interface, must be serializable
    /// </summary>
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
        /// <summary>
        /// Listen for events on channel and respond to them
        /// </summary>
        /// <param name="c">channel</param>
        void Handle(IChannel c);
    }
}