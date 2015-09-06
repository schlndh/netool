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
            if (ErrorOccured != null) ErrorOccured(this, e);
        }
    }
}