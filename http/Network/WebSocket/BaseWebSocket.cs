using Netool.Logging;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.WebSocket;
using System;

namespace Netool.Network.WebSocket
{
    /// <summary>
    /// Common functionality for websocket channels
    /// </summary>
    public class BaseWebSocket
    {
        private object contentLock = new object();
        private ThresholdedDataBuilder dataBuilder;
        private bool readingPayload = false;
        private WebSocketMessage.Parser parser = new WebSocketMessage.Parser();
        private InstanceLogger logger;
        private long missing;

        /// <summary>
        /// This event will be raised every time a new Message is sucessfully parsed.
        /// </summary>
        public event EventHandler<WebSocketMessage> MessageParsed;

        public BaseWebSocket(InstanceLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger;
            dataBuilder = new ThresholdedDataBuilder(logger);
        }

        private void onMessageParsed(WebSocketMessage m)
        {
            if (MessageParsed != null) MessageParsed(this, m);
        }

        /// <summary>
        /// Push new data into the parser
        /// </summary>
        /// <param name="s"></param>
        public void Receive(IDataStream s)
        {
            lock (contentLock)
            {
                while (s.Length > 0)
                {
                    while (!readingPayload && s.Length > 0)
                    {
                        missing = parser.ParseHeader(s);
                        if (missing <= 0)
                        {
                            var inner = s;
                            if (missing < 0)
                            {
                                var split = s.Length + missing;
                                inner = new StreamSegment(s, 0, split);
                                s = new StreamSegment(s, split);
                            }
                            else
                            {
                                s = EmptyData.Instance;
                            }
                            var ret = parser.Close(inner);
                            parser = new WebSocketMessage.Parser();
                            onMessageParsed(ret);
                        }
                        else
                        {
                            readingPayload = true;
                            dataBuilder.Append(s);
                            return;
                        }
                    }
                    if (readingPayload)
                    {
                        if (s.Length < missing)
                        {
                            dataBuilder.Append(s);
                            missing -= s.Length;
                            return;
                        }
                        else
                        {
                            dataBuilder.Append(new StreamSegment(s, 0, missing));
                            s = new StreamSegment(s, missing);
                            missing = 0;
                            readingPayload = false;
                            var bin = dataBuilder.Close();
                            dataBuilder = new ThresholdedDataBuilder(logger);
                            var msg = parser.Close(bin);
                            parser = new WebSocketMessage.Parser();
                            onMessageParsed(msg);
                        }
                    }
                }
            }
        }
    }
}