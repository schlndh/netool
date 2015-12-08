using Netool.Logging;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using System;

namespace Netool.Network.Http
{
    [Serializable]
    public class HttpMessageParserException : Exception { }

    [Serializable]
    public class HttpInvalidMessageException : Exception { }

    public class HttpMessageParser
    {
        private object contentLock = new object();
        private StreamList contentData = new StreamList();
        private ThresholdedDataBuilder dataBuilder;
        private long headerLength = 0;
        private long dechunkedLength = 0;

        private HttpHeaderParser parser = new HttpHeaderParser();
        private HttpBodyLengthInfo info;
        private bool readingBody = false;
        public HttpRequestMethod LastRequestMethod = HttpRequestMethod.Null;
        private InstanceLogger logger;
        private bool isResponse;

        public HttpMessageParser(InstanceLogger logger, bool isResponse)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger;
            this.isResponse = isResponse;
            dataBuilder = new ThresholdedDataBuilder(logger);
        }

        public HttpData Receive(IDataStream s)
        {
            lock (contentLock)
            {
                contentData.Add(s);
                dataBuilder.Append(s);
                if (!readingBody)
                {
                    try
                    {
                        if (parser.Parse(contentData, isResponse))
                        {
                            readingBody = true;
                            try
                            {
                                var code = parser.StatusCode;
                                // header parsing is finished
                                if (LastRequestMethod == HttpRequestMethod.HEAD)
                                {
                                    info = new HttpBodyLengthInfo((long)0);
                                }
                                else if (LastRequestMethod == HttpRequestMethod.CONNECT && code > 199 && code < 300)
                                {
                                    info = new HttpBodyLengthInfo((long)0);
                                }
                                else
                                {
                                    info = parser.GetBodyLengthInfo(!isResponse);
                                }
                                headerLength = parser.HeaderLength;
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
                                throw new HttpInvalidMessageException();
                            }
                        }
                    }
                    catch (HttpHeaderParserException)
                    {
                        throw new HttpInvalidMessageException();
                    }
                }
                if (readingBody)
                {
                    if (info.Type == HttpBodyLengthInfo.LengthType.Exact && info.Length <= dataBuilder.Length - headerLength)
                    {
                        var resStream = dataBuilder.Close();
                        return parser.Create(new StreamSegment(resStream, 0, headerLength), new StreamSegment(resStream, headerLength, info.Length), resStream);
                    }
                    else if (info.Type == HttpBodyLengthInfo.LengthType.Chunked)
                    {
                        if (contentData.Length == 0)
                        {
                            // wait for some data
                            return null;
                        }

                        var seg = new StreamSegment(contentData);
                        while(seg.Length > 0)
                        {
                            ChunkedDecoder.DecodeOneInfo chunkInfo;
                            try
                            {
                                chunkInfo = ChunkedDecoder.DecodeOneChunk(seg);
                            }
                            catch (PartialChunkException)
                            {
                                // wait for more data
                                contentData = new StreamList();
                                if (seg.Length > 0)
                                {
                                    contentData.Add(new ByteArray(seg));
                                }
                                return null;
                            }
                            catch
                            {
                                throw new HttpInvalidMessageException();
                            }
                            // finished
                            if(chunkInfo.DataLength == 0)
                            {
                                contentData = new StreamList();
                                var resStream = dataBuilder.Close();
                                return parser.Create(new StreamSegment(resStream, 0, headerLength), new DechunkedStream(new StreamSegment(resStream, headerLength), dechunkedLength), resStream);
                            }
                            else
                            {
                                dechunkedLength += chunkInfo.DataLength;
                                seg = new StreamSegment(contentData, seg.Offset + chunkInfo.ChunkLength, seg.Length - chunkInfo.ChunkLength);
                            }
                        }
                        contentData = new StreamList();
                    }
                }
            }
            return null;
        }

        public HttpData Close()
        {
            lock (contentLock)
            {
                if (readingBody && info.Type == HttpBodyLengthInfo.LengthType.CloseConnection)
                {
                    var resStream = dataBuilder.Close();
                    return parser.Create(new StreamSegment(resStream, 0, headerLength), new StreamSegment(resStream, headerLength), resStream);
                }
            }
            return null;
        }
    }
}