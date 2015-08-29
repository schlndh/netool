using Netool.Network;
using System;

namespace Netool.ChannelDrivers
{
    [Serializable]
    public class ChannelDriverException : Exception
    {
        public ChannelDriverException() { }
        public ChannelDriverException(string message) : base(message) { }
        public ChannelDriverException(string message, Exception inner) : base(message, inner) { }
        protected ChannelDriverException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class InvalidSettingsException : ChannelDriverException
    {
      public InvalidSettingsException() { }
      public InvalidSettingsException( string message ) : base( message ) { }
      public InvalidSettingsException( string message, Exception inner ) : base( message, inner ) { }
      protected InvalidSettingsException(
	    System.Runtime.Serialization.SerializationInfo info,
	    System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
    }

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
        /// Settings used to create this instance, can be null if the driver doesn't need any settings. Must be serializable
        /// </summary>
        object Settings { get; }
        /// <summary>
        /// Listen for events on channel and respond to them
        /// </summary>
        /// <param name="c">channel</param>
        void Handle(IChannel c);
    }
}