using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.Tcp;
using System;

namespace Netool.Network.Http
{
    [Serializable]
    public class HttpClientSettings
    {
        public TcpClientSettings TcpSettings;
    }

    [Serializable]
    public class HttpClientChannel : BaseClientChannel, IClientChannel
    {
        private IClientChannel channel;

        [NonSerialized]
        private ByteArray headerData = null;
        [NonSerialized]
        private StreamList contentData = new StreamList();
        [NonSerialized]
        private StreamList decodedChunkedData = new StreamList();
        [NonSerialized]
        private ChunkedDecoder chunkedDecoder = new ChunkedDecoder();

        [NonSerialized]
        private HttpHeaderParser parser = new HttpHeaderParser();
        [NonSerialized]
        private HttpBodyLengthInfo info;
        [NonSerialized]
        private HttpRequestMethod lastRequestMethod = HttpRequestMethod.Null;
        [NonSerialized]
        private bool readingResponseBody = false;

        public HttpClientChannel(IClientChannel channel)
        {
            this.channel = channel;
            this.id = channel.ID;
            this.name = channel.Name;
            channel.ChannelClosed += channelClosedHandler;
            channel.ChannelReady += channelReadyHandler;
            channel.ResponseReceived += responseReceivedHandler;
            channel.RequestSent += requestSentHandler;
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
        {
            lock (contentData)
            {
                contentData.Add(e.Data);
                if(!readingResponseBody)
                {
                    try
                    {
                        if (parser.ParseResponse(contentData))
                        {
                            readingResponseBody = true;
                            try
                            {
                                var code = parser.StatusCode;
                                // header parsing is finished
                                if (lastRequestMethod == HttpRequestMethod.HEAD)
                                {
                                    info = new HttpBodyLengthInfo((long)0);
                                }
                                else if (lastRequestMethod == HttpRequestMethod.CONNECT && code > 199 && code < 300)
                                {
                                    info = new HttpBodyLengthInfo((long)0);
                                }
                                else
                                {
                                    info = parser.GetBodyLengthInfo(false);
                                }
                                headerData = new ByteArray(contentData, 0, parser.HeaderLength);
                                if (contentData.Length - parser.HeaderLength > 0)
                                {
                                    var tmpList = new StreamList();
                                    var contentStart = new ByteArray(contentData, parser.HeaderLength);
                                    tmpList.Add(contentStart);
                                    contentData = tmpList;
                                }
                                else
                                {
                                    contentData = new StreamList();
                                }

                            }
                            catch (BadRequestException)
                            {
                                Close();
                            }
                        }
                    }
                    catch (HttpHeaderParserException)
                    {
                        // invalid response
                        Close();
                    }
                }
                if(readingResponseBody)
                {
                    if(info.Type == HttpBodyLengthInfo.LengthType.Exact && info.Length <= contentData.Length)
                    {
                        IDataStream bodyData = contentData;
                        if (contentData.Length > info.Length)
                        {
                            bodyData = new StreamSegment(contentData, 0, info.Length);
                        }
                        OnResponseReceived(parser.CreateResponse(headerData, bodyData));
                        resetReceiveStatus();
                    }
                    else if(info.Type == HttpBodyLengthInfo.LengthType.Chunked)
                    {
                        if(contentData.Length == 0)
                        {
                            // wait for some data
                            return;
                        }
                        try
                        {
                            var chunkInfo = chunkedDecoder.Decode(contentData);
                            contentData = new StreamList();
                            if (chunkInfo.DecodedData.Length > 0)
                            {
                                decodedChunkedData.Add(chunkInfo.DecodedData);
                            }
                            if (chunkInfo.Finished)
                            {
                                OnResponseReceived(parser.CreateResponse(headerData, decodedChunkedData));
                                resetReceiveStatus();
                            }
                        }
                        catch(PartialChunkException)
                        {
                            // wait for more data
                            return;
                        }
                        catch
                        {
                            // invalid response
                            Close();
                        }
                    }
                }

            }
        }

        private void requestSentHandler(object sender, DataEventArgs e)
        {
            OnRequestSent(e.Data, e.State);
        }

        private void channelReadyHandler(object sender)
        {
            OnChannelReady();
        }

        private void channelClosedHandler(object sender)
        {
            lock(contentData)
            {
                if(readingResponseBody && info.Type == HttpBodyLengthInfo.LengthType.CloseConnection)
                {
                    OnResponseReceived(parser.CreateResponse(headerData, contentData));
                    resetReceiveStatus();
                }
            }
            OnChannelClosed();
        }

        /// <inheritdoc/>
        public void Send(IDataStream request)
        {
            var http = request as HttpData;
            if(http != null)
            {

            }
            channel.Send(request);
        }

        /// <inheritdoc/>
        public void Close()
        {
            channel.Close();
        }

        private void resetReceiveStatus()
        {
            chunkedDecoder = new ChunkedDecoder();
            decodedChunkedData = new StreamList();
            contentData = new StreamList();
            readingResponseBody = false;
            lastRequestMethod = HttpRequestMethod.Null;
            parser = new HttpHeaderParser();
        }
    }

    [Serializable]
    public class HttpClient : IClient
    {
        protected HttpClientSettings setttings;
        protected TcpClient client;
        private HttpClientChannel channel;

        /// <inheritdoc/>
        public bool IsStarted { get { return client.IsStarted; } }

        /// <inheritdoc/>
        public object Settings { get { return setttings; } }

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<IClientChannel> ChannelCreated;

        public HttpClient(HttpClientSettings settings)
        {
            this.setttings = settings;
            client = new TcpClient(settings.TcpSettings);
            client.ChannelCreated += channelCreatedHandler;
        }

        private void channelCreatedHandler(object sender, IClientChannel e)
        {
            channel = new HttpClientChannel(e);
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        /// <inheritdoc/>
        public IClientChannel Start()
        {
            var c = client.Start();
            return channel;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            client.Stop();
        }
    }
}