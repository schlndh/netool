using System;

namespace Netool.Network
{
    [Serializable]
    public abstract class BaseInstance
    {
        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<Exception> ErrorOccured;

        protected virtual void OnErrorOccured(Exception e)
        {
            var ev = ErrorOccured;
            if (ev != null) ev(this, e);
        }
    }
}