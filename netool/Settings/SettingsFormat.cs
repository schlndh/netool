using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
namespace Netool.Settings
{
    public enum InstanceType { Client, Server, Proxy}
    public enum BaseProtocol { Udp, Tcp}
    [Serializable()]
    public class BaseSettings { }
    [Serializable()]
    public class InstanceSettings: BaseSettings
    {
        public string PluginName;
        public BaseProtocol Protocol;
        public InstanceType Type;
        public BaseSettings PluginData;
    }
}
