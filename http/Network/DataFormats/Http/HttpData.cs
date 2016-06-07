using System;
using System.Collections.Generic;
using System.Text;

namespace Netool.Network.DataFormats.Http
{
    /// <summary>
    /// Standard Http request methods, Null
    /// </summary>
    public enum HttpRequestMethod { Null, GET, HEAD, POST, PUT, DELETE, CONNECT, OPTIONS, TRACE }

    [Serializable]
    public class HttpData : IDataStream
    {
        /// <summary>
        /// Used to build HttpData objects
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        public class Builder
        {
            private Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private List<string> headerKeys = new List<string>();

            public bool IsRequest { get; set; }
            public string ReasonPhrase { get; set; }
            public int StatusCode { get; set; }
            public string HttpVersion { get; set; }
            public HttpRequestMethod Method { get; set; }
            public string RequestTarget { get; set; }

            /// <summary>
            /// Adds a header to header collection,
            /// if the header is already present it appends the value separated by a comma
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void AddHeader(string key, string value)
            {
                if (headers.ContainsKey(key))
                {
                    headers[key] = headers[key] + "," + value;
                }
                else
                {
                    headers[key] = value;
                    headerKeys.Add(key);
                }
            }

            /// <summary>
            /// Creates a HttpData from current state and reset the state
            /// </summary>
            /// <param name="payload"></param>
            /// <param name="headerData"></param>
            /// <param name="messageData"></param>
            /// <returns></returns>
            /// <remarks>
            /// This method doesn't copy the headers
            /// </remarks>
            public HttpData CreateAndClear(IDataStream payload = null, IDataStream headerData = null, IDataStream messageData = null)
            {
                HttpData data;
                if (IsRequest)
                {
                    data = HttpData.CreateRequest(headers, headerKeys, HttpVersion, Method, RequestTarget, payload, headerData, messageData);
                }
                else
                {
                    data = HttpData.CreateResponse(headers, headerKeys, HttpVersion, StatusCode, ReasonPhrase, payload, headerData, messageData);
                }

                headers = new Dictionary<string, string>();
                headerKeys = new List<string>();
                HttpVersion = null;
                StatusCode = 0;
                ReasonPhrase = null;
                IsRequest = false;
                Method = HttpRequestMethod.Null;
                RequestTarget = null;
                return data;
            }

            /// <summary>
            /// Gets header from already parsed data
            /// </summary>
            /// <param name="key"></param>
            /// <returns>header value or null if header wasn't found</returns>
            public string GetHeader(string key)
            {
                try
                {
                    return headers[key];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
        }

        public readonly IReadOnlyDictionary<string, string> Headers;
        public readonly IReadOnlyList<string> HeaderKeys;
        public readonly bool IsRequest;
        public readonly IDataStream MessageData;
        public readonly IDataStream HeaderData;
        public readonly IDataStream BodyData;

        /// <summary>
        /// HTTP version string
        /// </summary>
        public readonly string Version;

        /// <summary>
        /// HTTP response code (only for response)
        /// </summary>
        public readonly int Code;

        /// <summary>
        /// HTTP reason phrase (only for response)
        /// </summary>
        public readonly string ReasonPhrase;

        /// <summary>
        /// HTTP request method (only for request)
        /// </summary>
        public readonly HttpRequestMethod Method;
        /// <summary>
        /// HTTP request target (only for request)
        /// </summary>
        public readonly string RequestTarget;

        /// <summary>
        /// Return normalized status line corresponding to response/request without trailing CRLF
        /// </summary>
        public string StatusLine
        {
            get
            {
                if(IsRequest)
                {
                    return Method.ToString() + " " + RequestTarget + " HTTP/" + Version;
                }
                else
                {
                    return "HTTP/" + Version + " " + Code.ToString() + " " + ReasonPhrase;
                }
            }
        }

        /// <inheritdoc/>
        public long Length { get { return MessageData.Length; } }

        private HttpData(IDataStream headerData, IReadOnlyDictionary<string, string> headers, IReadOnlyList<string> headerKeys, bool isRequest, string version, int code, string reasonPhrase, HttpRequestMethod method, string requestTarget, IDataStream bodyData = null, IDataStream messageData = null)
        {
            HeaderData = headerData;
            BodyData = bodyData ?? EmptyData.Instance;
            if(messageData == null)
            {
                var list = new StreamList();
                list.Add(HeaderData);
                list.Add(BodyData);
                list.Freeze();
                MessageData = list;
            }
            else
            {
                MessageData = messageData;
            }
            Headers = headers;
            HeaderKeys = headerKeys;
            IsRequest = isRequest;
            Version = version;
            Code = code;
            ReasonPhrase = reasonPhrase;
            Method = method;
            RequestTarget = requestTarget;
        }

        public static HttpData CreateResponse(IReadOnlyDictionary<string, string> headers,
            IReadOnlyList<string> headerKeys, string version, int code, string reasonPhrase,
            IDataStream payload = null, IDataStream headerData = null, IDataStream messageData = null)
        {
            if(headerData == null)
            {
                var stringBuilder = new StringBuilder("HTTP/" + version + " " + code.ToString() + " " + reasonPhrase + "\r\n");
                foreach (var key in headerKeys)
                {
                    stringBuilder.Append(key + ":" + headers[key] + "\r\n");
                }
                stringBuilder.Append("\r\n");
                headerData = new ByteArray(ASCIIEncoding.ASCII.GetBytes(stringBuilder.ToString()));
            }

            return new HttpData(headerData, headers, headerKeys, false, version, code, reasonPhrase, HttpRequestMethod.Null, "", payload, messageData);
        }

        public static HttpData CreateRequest(IReadOnlyDictionary<string, string> headers,
            IReadOnlyList<string> headerKeys, string version, HttpRequestMethod method,
            string requestTarget, IDataStream payload = null, IDataStream headerData = null, IDataStream messageData = null)
        {
            if(headerData == null)
            {
                var stringBuilder = new StringBuilder(method.ToString() + " " + requestTarget + " HTTP/" + version + "\r\n");
                foreach (var key in headerKeys)
                {
                    stringBuilder.Append(key + ":" + headers[key] + "\r\n");
                }
                stringBuilder.Append("\r\n");
                headerData = new ByteArray(ASCIIEncoding.ASCII.GetBytes(stringBuilder.ToString()));
            }

            return new HttpData(headerData, headers, headerKeys, true, version, -1, "", method, requestTarget, payload, messageData);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this;
        }

        /// <inheritdoc/>
        public byte ReadByte(long index)
        {
            return MessageData.ReadByte(index);
        }

        /// <inheritdoc/>
        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            MessageData.ReadBytesToBuffer(buffer, start, length, offset);
        }

        public override string ToString()
        {
            return StatusLine;
        }
    }
}