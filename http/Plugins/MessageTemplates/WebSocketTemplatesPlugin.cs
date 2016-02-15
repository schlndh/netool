using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using System;
using System.Collections.Generic;

namespace Netool.Plugins.MessageTemplates
{
    public class WebSocketTemplatesPlugin : IMessageTemplatePlugin
    {
        public class WebSocketHandshakeClientTemplate : IMessageTemplate
        {
            /// <inheritdoc/>
            public string Name { get { return "WebSocket/Http/ClientHandshake"; } }

            private Random gen = new Random();

            /// <inheritdoc/>
            public IDataStream CreateMessage()
            {
                var key = new byte[16];
                gen.NextBytes(key);
                var builder = new HttpData.Builder();
                builder.IsRequest = true;
                builder.HttpVersion = "1.1";
                builder.Method = HttpRequestMethod.GET;
                builder.RequestTarget = "/";
                builder.AddHeader("Host", "");
                builder.AddHeader("Upgrade", "websocket");
                builder.AddHeader("Connection", "Upgrade");
                builder.AddHeader("Sec-WebSocket-Version", "13");
                builder.AddHeader("Sec-WebSocket-Key", Convert.ToBase64String(key));
                return builder.CreateAndClear();
            }
        }

        /// <inheritdoc/>
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc/>
        public string Description { get { return "Message templates for WebSocket."; } }

        /// <inheritdoc/>
        public long ID { get { return 12001; } }

        /// <inheritdoc/>
        public string Name { get { return "WebSocketTemplatesPlugin"; } }

        /// <inheritdoc/>
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc/>
        public IEnumerable<IMessageTemplate> CreateTemplates()
        {
            return new IMessageTemplate[] { new WebSocketHandshakeClientTemplate() };
        }
    }
}