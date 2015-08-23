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
        private ByteArray headerData = null;
        private StreamList contentData = new StreamList();
        private StreamList decodedChunkedData = new StreamList();
        private ChunkedDecoder chunkedDecoder = new ChunkedDecoder();

        private HttpHeaderParser parser = new HttpHeaderParser();
        private HttpBodyLengthInfo info;
        private bool readingBody = false;
        public HttpRequestMethod LastRequestMethod = HttpRequestMethod.Null;

        private bool isResponse;

        public HttpMessageParser(bool isResponse)
        {
            this.isResponse = isResponse;
        }

        public HttpData Receive(IDataStream s)
        {
            lock (contentData)
            {
                contentData.Add(s);
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
                    if (info.Type == HttpBodyLengthInfo.LengthType.Exact && info.Length <= contentData.Length)
                    {
                        IDataStream bodyData = contentData;
                        if (contentData.Length > info.Length)
                        {
                            bodyData = new StreamSegment(contentData, 0, info.Length);
                        }
                        return parser.Create(headerData, bodyData);
                    }
                    else if (info.Type == HttpBodyLengthInfo.LengthType.Chunked)
                    {
                        if (contentData.Length == 0)
                        {
                            // wait for some data
                            return null;
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
                                return parser.Create(headerData, decodedChunkedData);
                            }
                        }
                        catch (PartialChunkException)
                        {
                            // wait for more data
                            return null;
                        }
                        catch
                        {
                            throw new HttpInvalidMessageException();
                        }
                    }
                }
            }
            return null;
        }

        public HttpData Close()
        {
            lock (contentData)
            {
                if (readingBody && info.Type == HttpBodyLengthInfo.LengthType.CloseConnection)
                {
                    return parser.Create(headerData, contentData);
                }
            }
            return null;
        }
    }
}