using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netool
{
        [Serializable]
        public class UnknownPluginException : System.Exception
        {
            private long pluginID;
            public long PluginID { get { return pluginID; } }
            public UnknownPluginException(long pluginID)
            {
                this.pluginID = pluginID;
            }
        }
}
