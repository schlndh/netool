using System;

namespace Netool.Plugins
{
    [Serializable]
    public class PluginException : Exception
    {
        public PluginException() { }
        public PluginException(string message) : base(message) { }
        public PluginException(string message, Exception inner) : base(message, inner) { }
        protected PluginException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class SetupAbortedByUserException : PluginException
    {
        public SetupAbortedByUserException() { }
        public SetupAbortedByUserException(string message) : base(message) { }
        public SetupAbortedByUserException(string message, Exception inner) : base(message, inner) { }
        protected SetupAbortedByUserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class InvalidSettingsTypeException : PluginException
    {
        public InvalidSettingsTypeException() { }
        public InvalidSettingsTypeException(string message) : base(message) { }
        public InvalidSettingsTypeException(string message, Exception inner) : base(message, inner) { }
        protected InvalidSettingsTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}