﻿using Netool.Logging;
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
        private enum ParsingState { Header, Body, CheckingChunedTrailer, ParsingChunkedTrailer };
        private object contentLock = new object();
        private StreamList contentData = new StreamList();
        private ThresholdedDataBuilder dataBuilder;
        private long headerLength = 0;
        private long dechunkedLength = 0;

        private HttpHeaderParser parser = new HttpHeaderParser();
        private HttpBodyLengthInfo info;
        private ParsingState state = ParsingState.Header;
        public HttpRequestMethod LastRequestMethod = HttpRequestMethod.Null;
        private InstanceLogger logger;
        private bool isResponse;
        private bool useContentData = true;

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
                if(useContentData) contentData.Add(s);
                dataBuilder.Append(s);
                if (state == ParsingState.Header)
                {
                    try
                    {
                        if (parser.Parse(contentData, isResponse))
                        {
                            state = ParsingState.Body;
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
                                useContentData = info.Type == HttpBodyLengthInfo.LengthType.Chunked;
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
                if (state == ParsingState.Body)
                {
                    if (info.Type == HttpBodyLengthInfo.LengthType.Exact && info.Length <= dataBuilder.Length - headerLength)
                    {
                        var resStream = dataBuilder.Close();
                        return parser.Create(new StreamSegment(resStream, 0, headerLength), new StreamSegment(resStream, headerLength, info.Length), resStream);
                    }
                    else if (info.Type == HttpBodyLengthInfo.LengthType.Chunked)
                    {
                        parseChunkedBody();
                    }
                }
            }
            if(state == ParsingState.CheckingChunedTrailer)
            {
                if(contentData.Length >= 2)
                {
                    // check CRLF
                    bool chunkedEnd = contentData.ReadByte(0) == 13 && contentData.ReadByte(1) == 10;
                    if (chunkedEnd)
                    {
                        contentData = new StreamList();
                        var resStream = dataBuilder.Close();
                        return parser.Create(new StreamSegment(resStream, 0, headerLength), new StreamSegment(resStream, headerLength), resStream);
                    }
                    else
                    {
                        parser.ParseChunkedTrailer();
                        state = ParsingState.ParsingChunkedTrailer;
                    }
                }
            }
            if(state == ParsingState.ParsingChunkedTrailer)
            {
                if(parser.Parse(contentData, isResponse))
                {
                    contentData = new StreamList();
                    var resStream = dataBuilder.Close();
                    return parser.Create(new StreamSegment(resStream, 0, headerLength), new StreamSegment(resStream, headerLength), resStream);
                }
            }
            return null;
        }

        public HttpData Close()
        {
            lock (contentLock)
            {
                if (state == ParsingState.Body && info.Type == HttpBodyLengthInfo.LengthType.CloseConnection)
                {
                    var resStream = dataBuilder.Close();
                    return parser.Create(new StreamSegment(resStream, 0, headerLength), new StreamSegment(resStream, headerLength), resStream);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets raw binary data acumulated in the parser.
        /// </summary>
        /// <remarks>
        /// You can use this method to obtain received data if an exception is throw from Receive method.
        /// This method invalidates the parser and it cannot be used after calling it.
        /// </remarks>
        /// <returns></returns>
        public IDataStream GetRawData()
        {
            lock(contentLock)
            {
                return dataBuilder.Close();
            }
        }

        private void parseChunkedBody()
        {
            if (contentData.Length == 0)
            {
                // wait for some data
                return;
            }

            var seg = new StreamSegment(contentData);
            while (seg.Length > 0)
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
                    return;
                }
                catch
                {
                    throw new HttpInvalidMessageException();
                }
                // finished
                if (chunkInfo.DataLength == 0)
                {
                    seg = new StreamSegment(seg, chunkInfo.ChunkLength);
                    contentData = new StreamList();
                    contentData.Add(new ByteArray(seg));
                    state = ParsingState.CheckingChunedTrailer;
                    return;
                }
                else
                {
                    dechunkedLength += chunkInfo.DataLength;
                    seg = new StreamSegment(contentData, seg.Offset + chunkInfo.ChunkLength, seg.Length - chunkInfo.ChunkLength);
                }
            }
            contentData = new StreamList();
            return;
        }
    }
}