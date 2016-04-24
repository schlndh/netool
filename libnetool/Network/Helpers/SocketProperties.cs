using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Netool.Network.Helpers
{
    [Serializable]
    public class SocketProperties : ICloneable
    {
        private static SocketProperties DefaultProperties = new SocketProperties();

        public bool DontFragment { get; set; }

        public bool DualMode { get; set; }

        public bool EnableBroadcast { get; set; }

        public bool MulticastLoopback { get; set; }

        public bool NoDelay { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int ReceiveTimeout { get; set; }

        public int SendBufferSize { get; set; }

        public int SendTimeout { get; set; }

        public short Ttl { get; set; }

        private delegate void VoidDelegate();

        public SocketProperties()
        {
            DontFragment = false;
            DualMode = false;
            EnableBroadcast = false;
            MulticastLoopback = false;
            NoDelay = false;
            ReceiveBufferSize = 8192;
            ReceiveTimeout = 0;
            SendBufferSize = 8192;
            SendTimeout = 0;
            Ttl = 32;
        }

        public void Apply(Socket s)
        {
            ignoreExceptions(() => s.DontFragment = DontFragment);
            ignoreExceptions(() => s.DualMode = DualMode);
            ignoreExceptions(() => s.EnableBroadcast = EnableBroadcast);
            ignoreExceptions(() => s.MulticastLoopback = MulticastLoopback);
            ignoreExceptions(() => s.NoDelay = NoDelay);
            ignoreExceptions(() => s.ReceiveBufferSize = ReceiveBufferSize);
            ignoreExceptions(() => s.ReceiveTimeout = ReceiveTimeout);
            ignoreExceptions(() => s.SendBufferSize = SendBufferSize);
            ignoreExceptions(() => s.SendTimeout = SendTimeout);
            ignoreExceptions(() => s.Ttl = Ttl);
        }

        private void ignoreExceptions(VoidDelegate d)
        {
            try
            {
                d();
            }
            catch { }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            List<string> parts = new List<string>(10);
            var props = typeof(SocketProperties).GetProperties();
            foreach (var prop in props)
            {
                if (!prop.GetValue(DefaultProperties).Equals(prop.GetValue(this)))
                {
                    parts.Add(string.Format("{0}={1}", prop.Name, prop.GetValue(this)));
                }
            }
            if (parts.Count == 0) return "(default)";
            return "(" + string.Join(", ", parts.ToArray()) + ")";
        }
    }
}