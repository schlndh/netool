using Netool.Dialogs;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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

        public class WebSocketHandshakeServerTemplate : IMessageTemplate
        {
            public string Name { get { return "WebSocket/Http/ServerHandshake"; } }

            private static WebSocketHandshakeServerTemplate instance;
            public static WebSocketHandshakeServerTemplate Instance { get { if (instance == null) instance = new WebSocketHandshakeServerTemplate(); return instance; } }
            private WebSocketHandshakeServerTemplate() {  }

            /// <inheritdoc/>
            public IDataStream CreateMessage()
            {
                var dialog = new TextBoxDialog("Enter the value of Sec-WebSocket-Key from client request.");
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return CreateMessage(dialog.Value);
                }
                return CreateMessage(null);
            }

            /// <summary>
            /// Non-interactive interface for CreateMessage method
            /// </summary>
            /// <param name="SecWebsocketKey">null if you don't wish to add Sec-WebSocket-Accept header to the template</param>
            /// <returns></returns>
            public IDataStream CreateMessage(string SecWebsocketKey)
            {
                var builder = new HttpData.Builder();
                builder.IsRequest = false;
                builder.HttpVersion = "1.1";
                builder.StatusCode = 101;
                builder.ReasonPhrase = "Switching Protocols";
                builder.AddHeader("Host", "");
                builder.AddHeader("Upgrade", "websocket");
                builder.AddHeader("Connection", "Upgrade");
                builder.AddHeader("Sec-WebSocket-Version", "13");
                if(SecWebsocketKey != null)
                {
                    var sha1 = new SHA1Managed();
                    var key = Convert.ToBase64String(sha1.ComputeHash(ASCIIEncoding.ASCII.GetBytes(SecWebsocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                    builder.AddHeader("Sec-WebSocket-Accept", key);
                }
                
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
            return new IMessageTemplate[] 
            {
                new WebSocketHandshakeClientTemplate(),
                WebSocketHandshakeServerTemplate.Instance,
            };
        }
    }
}