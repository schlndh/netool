using CSScriptLibrary;
using Netool.Network;
using System;

namespace Netool.ChannelDrivers
{
    [Serializable]
    public class CSScriptChannelDriver : IChannelDriver
    {
        /// <inheritdoc/>
        public bool AllowManualControl { get { return innerDriver == null || innerDriver.AllowManualControl; } }

        /// <inheritdoc/>
        public string ID { get { return "CS-Script"; } }

        /// <inheritdoc/>
        public object Settings { get { return scriptFilename; } }

        [NonSerialized]
        private IChannelDriver innerDriver;

        private string scriptFilename;

        public CSScriptChannelDriver(string filename)
        {
            try
            {
                innerDriver = (IChannelDriver)CSScriptLibrary.CSScript
                    .LoadCodeFrom(filename)
                    .CreateObject("*")
                    .AlignToInterface<IChannelDriver>(); ;
                scriptFilename = filename;
            }
            catch(Exception e)
            {
                throw new InvalidSettingsException("Unable to load class from given file!", e);
            }

        }

        /// <inheritdoc/>
        public bool CanAccept(IChannel c)
        {
            return innerDriver.CanAccept(c);
        }

        /// <inheritdoc/>
        public void Handle(IChannel c)
        {
            innerDriver.Handle(c);
        }
    }
}