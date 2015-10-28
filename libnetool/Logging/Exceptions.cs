using System;

namespace Netool.Logging
{
    [Serializable]
    public class LoggingException : Exception
    {
        public LoggingException()
        {
        }

        public LoggingException(string message)
            : base(message)
        {
        }

        public LoggingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected LoggingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class DeserializationException : LoggingException
    {
        public DeserializationException()
        {
        }

        public DeserializationException(string message)
            : base(message)
        {
        }

        public DeserializationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DeserializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class InvalidContextException : DeserializationException
    {
        public InvalidContextException()
        {
        }

        public InvalidContextException(string message)
            : base(message)
        {
        }

        public InvalidContextException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidContextException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BufferNotLargeEnoughException : LoggingException
    {
        public BufferNotLargeEnoughException() { }
        public BufferNotLargeEnoughException(string message) : base(message) { }
        public BufferNotLargeEnoughException(string message, Exception inner) : base(message, inner) { }
        protected BufferNotLargeEnoughException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class LoggedFileCorruptedException : LoggingException
    {
        public LoggedFileCorruptedException() { }
        public LoggedFileCorruptedException(string message) : base(message) { }
        public LoggedFileCorruptedException(string message, Exception inner) : base(message, inner) { }
        protected LoggedFileCorruptedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}