using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netool;
using Netool.Settings;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Collections.Generic;
namespace Tests
{
    [Serializable()]
    internal class PluginSettings : BaseSettings
    {
        public string Whatever = "aaa";
    }
    [TestClass]
    public class SettingsFormatTests
    {
        [TestMethod]
        public void PersistenceTest()
        {
            var ins = new InstanceSettings();
            ins.PluginName = "aaa";
            ins.Protocol = BaseProtocol.Tcp;
            ins.Type = InstanceType.Client;
            var pl = new PluginSettings();
            pl.Whatever = "bbb";
            ins.PluginData = pl;
            var types = new List<Type> { typeof(InstanceSettings), typeof(PluginSettings) };
            var ser = new DataContractSerializer(typeof(BaseSettings), types);
            using (var stream = new MemoryStream())
            {
                ser.WriteObject(stream, ins);
                // move back to the begining of the stream
                stream.Seek(0, SeekOrigin.Begin);
                InstanceSettings ins2;
                ins2 = (InstanceSettings)ser.ReadObject(stream);
                Assert.AreEqual(ins.PluginName, ins2.PluginName);
                Assert.AreEqual(ins.Protocol, ins2.Protocol);
                Assert.AreEqual(ins.Type, ins2.Type);
                Assert.IsInstanceOfType(ins2.PluginData, typeof(PluginSettings));
                Assert.AreEqual(((PluginSettings)ins.PluginData).Whatever, ((PluginSettings)ins2.PluginData).Whatever);
            }
        }
    }
}
