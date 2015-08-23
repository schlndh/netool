using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Netool.Network.DataFormats.Http
{
    [Serializable]
    public class HttpHeaderParserException : Exception { }

    [Serializable]
    public class InvalidHttpHeaderException : HttpHeaderParserException { }

    [Serializable]
    public class ParserFinishedException : HttpHeaderParserException { }

    [Serializable]
    public class ParsingNotFinishedException : HttpHeaderParserException { }

    [Serializable]
    public class BadRequestException : HttpHeaderParserException {}

    [Serializable]
    public struct HttpBodyLengthInfo
    {
        public enum LengthType { Null, Exact, CloseConnection, Chunked }
        public readonly LengthType Type;
        public readonly long Length;

        public HttpBodyLengthInfo(LengthType type, long length)
        {
            Type = type;
            Length = length;
        }

        public HttpBodyLengthInfo(long length)
        {
            Type = LengthType.Exact;
            Length = length;
        }

        public HttpBodyLengthInfo(LengthType type)
        {
            Type = type;
            Length = -1;
        }
    }

    public class HttpHeaderParser
    {
        private enum ParsingStage { StatusLine, Headers, Finished };

        private ParsingStage stage = ParsingStage.StatusLine;
        private HttpData.Builder builder = new HttpData.Builder();
        private int nextRead = 0;

        public int StatusCode { get { lock (builder) { return builder.StatusCode; } } }
        /// <summary>
        /// Get amount of header bytes read so far
        /// </summary>
        public int HeaderLength { get { lock (builder) { return nextRead; } } }

        private static string obsFoldPt = "\r\n[ \t]+";
        private static string tokenPt = @"[!#$%'*+\-.^_`|~\da-zA-Z]+";
        private static string owsPt = "[\t ]*";
        private static string obsTextPt = @"[\x80-\xFF]";
        private static string vcharPt = @"[\x21-\x7E]";

        /*private static string unreservedCharPt = @"[-a-zA-Z0-9._~]";
        private static string pctEncodedPt = @"(?:%[0-9A-F]{2})";
        private static string subDelimsPt = @"[!$&'()*+,;=]";
        private static string schemePt = @"[a-zA-Z](?:[-a-zA-Z0-9+.])*";
        private static string userInfoPt = @"(?:" + unreservedCharPt + "|" + pctEncodedPt + "|" + subDelimsPt + "|:)*";
        private static string pcharPt = @"(?:" + unreservedCharPt + "|" + pctEncodedPt + "|" + subDelimsPt + "|[:@])";
        private static string absolutePathPt = @"(?:/" + pcharPt + "*)+";
        private static string queryPt = @"(?:" + pcharPt + "|[/?])*";
        private static string hostPt = ""
        private static string authorityPt = @"(?:" + userInfoPt + @"@)?" + hostPt + "(?::" + portPt +")?";
        private static string hierPartPt = @"(?://" + authorityPt + pathAbEmpty + "|" + pathAbsolutePt + "|" + pathRootless + "|" + pathEmptyPt + ")";
        private static string originFormPt = absolutePathPt + @"(?:\?" + queryPt + @")\?";
        private static string absoluteURIPt = schemePt + ":" + hierPartPt + @"(?:\?" + queryPt + ")?";
        private static string absoluteFormPt = absoluteURIPt;
        private static string requestTargetPt = "@(?:" + originFormPt + "|" + absoluteFormPt + "|" + authorityFormPt + "|" + asteriskFormPt + ")";*/
        private static string statusLinePt = @"^HTTP/(?<Version>\d\.\d) (?<Code>\d{3}) (?<ReasonPhrase>[ " + '\t' + @"\x21-\x7E\x80-\xFF]*)" + "\r\n";
        private static string requestLinePt = @"^(?<Method>" + tokenPt + @") (?<Target>[^ ]+) HTTP/(?<Version>\d\.\d)" + "\r\n";
        private static string fieldVcharPt = "(?:" + vcharPt + "|" + obsTextPt + ")";
        private static string fieldContentPt = fieldVcharPt + "(?:[ \t]+" + fieldVcharPt + ")?";
        private static string fieldValuePt = @"(?:" + fieldContentPt + "|" + obsFoldPt + ")*";
        private static string partialHeaderFieldPt = @"\G(?<Key>" + tokenPt + ")(?<Colon>:)?" + owsPt + "(?<Value>" + fieldValuePt + ")?" + owsPt;

        private static Regex statusLineRegex = new Regex(statusLinePt);
        private static Regex requestLineRegex = new Regex(requestLinePt);
        private static Regex headerRegex = new Regex(partialHeaderFieldPt);

        public static HttpData.Builder ParseStartLine(string str)
        {
            var builder = new HttpData.Builder();
            var match = statusLineRegex.Match(str);
            if (match.Success)
            {
                builder.IsRequest = false;
                builder.HttpVersion = match.Groups["Version"].Value;
                builder.StatusCode = int.Parse(match.Groups["Code"].Value);
                builder.ReasonPhrase = match.Groups["ReasonPhrase"].Value;
            }
            else
            {
                match = requestLineRegex.Match(str);
                if (!match.Success) throw new InvalidHttpHeaderException();
                var target = match.Groups["Target"].Value;
                HttpRequestMethod method;
                if (!Enum.TryParse(match.Groups["Method"].Value, out method)) throw new InvalidHttpHeaderException();
                builder.IsRequest = true;
                builder.HttpVersion = match.Groups["Version"].Value;
                builder.Method = method;
                builder.RequestTarget = target;
            }

            return builder;
        }

        /// <summary>
        /// Parses response/request header
        /// </summary>
        /// <param name="s">data to parse, must be long enough to contain at least the start line</param>
        /// <param name="isResponse"></param>
        /// <returns>is header parsing finished</returns>
        /// <exception cref="InvalidHttpHeaderException">Thrown when parsing is unsuccessful</exception>
        /// <remarks>Max headers size is limited by 512KiB</remarks>
        public bool Parse(IDataStream s, bool isResponse)
        {
            lock(builder)
            {
                if (stage == ParsingStage.Finished) return true;
                if (s.Length <= nextRead) return false;

                byte[] arr;
                // max 512KiB
                int len = (int)Math.Min(s.Length - nextRead, 524288);
                arr = s.ReadBytes(nextRead, len);
                var str = ASCIIEncoding.ASCII.GetString(arr);
                if (stage == ParsingStage.StatusLine)
                {
                    if(isResponse)
                    {
                        var match = statusLineRegex.Match(str);
                        if (!match.Success && str.Length < s.Length) throw new InvalidHttpHeaderException();

                        builder.IsRequest = false;
                        builder.HttpVersion = match.Groups["Version"].Value;
                        builder.StatusCode = int.Parse(match.Groups["Code"].Value);
                        builder.ReasonPhrase = match.Groups["ReasonPhrase"].Value;
                        nextRead = match.Length;
                        stage = ParsingStage.Headers;
                        str = str.Substring(nextRead);
                    }
                    else
                    {
                        var match = requestLineRegex.Match(str);
                        if (!match.Success && str.Length < s.Length) throw new InvalidHttpHeaderException();

                        var target = match.Groups["Target"].Value;
                        HttpRequestMethod method;
                        if (!Enum.TryParse(match.Groups["Method"].Value, out method)) throw new InvalidHttpHeaderException();
                        builder.IsRequest = true;
                        builder.HttpVersion = match.Groups["Version"].Value;
                        builder.Method = method;
                        builder.RequestTarget = target;
                        nextRead = match.Length;
                        stage = ParsingStage.Headers;
                        str = str.Substring(nextRead);
                    }
                }
                if (stage == ParsingStage.Headers)
                {
                    int read = 0;
                    Match match;
                    while ((match = headerRegex.Match(str, read)).Success)
                    {
                        // Check for end of the string
                        if (str.Length - (match.Length + read) < 2)
                        {
                            nextRead += read;
                            return false;
                        }
                        var key = match.Groups["Key"].Value;
                        var value = match.Groups["Value"].Value;
                        builder.AddHeader(key, value);
                        read += match.Length;
                        if (str.Substring(read, 2) != "\r\n")
                        {
                            throw new InvalidHttpHeaderException();
                        }
                        read += 2;
                    }
                    if (str.Length > 0 && read == 0) throw new InvalidHttpHeaderException();
                    str = str.Substring(read);
                    nextRead += read;
                }
                if (stage != ParsingStage.Finished && str.Length >= 2 && str.Substring(0, 2) == "\r\n")
                {
                    stage = ParsingStage.Finished;
                    nextRead += 2;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Creates a response from current parser
        /// </summary>
        /// <param name="headerData"></param>
        /// <param name="bodyData"></param>
        /// <returns>parsed http response</returns>
        /// <exception cref="ParsingNotFinishedException">if this method is called before parsing is finished</exception>
        public HttpData Create(IDataStream headerData, IDataStream bodyData)
        {
            lock(builder)
            {
                if (stage == ParsingStage.Finished)
                {
                    return builder.CreateAndClear(bodyData, headerData);
                }
                else
                {
                    throw new ParsingNotFinishedException();
                }
            }
        }

        /// <inheritdoc cref="HttpData.Builder.GetHeader"/>
        public string GetHeader(string key)
        {
            lock(builder)
            {
                return builder.GetHeader(key);
            }
        }

        /// <summary>
        /// Gets information about body length
        /// </summary>
        /// <param name="isRequest"></param>
        /// <returns></returns>
        /// <exception cref="BadRequestException">when response/request body length cannot be determined (http error no 400)</exception>
        /// <remarks>
        /// Relevant RFC link: https://tools.ietf.org/html/rfc7230#section-3.3.3
        /// </remarks>
        public HttpBodyLengthInfo GetBodyLengthInfo(bool isRequest)
        {
            lock(builder)
            {
                if(stage == ParsingStage.Finished)
                {
                    if (!isRequest)
                    {
                        // 1xx - informational, 204 - no content, 304 - not modified
                        if((builder.StatusCode >= 100 && builder.StatusCode < 200) || builder.StatusCode == 204 || builder.StatusCode == 304)
                        {
                            return new HttpBodyLengthInfo((long)0);
                        }
                    }

                    var encoding = builder.GetHeader("Transfer-Encoding");
                    var contentLength = builder.GetHeader("Content-Length");
                    if(encoding != null)
                    {
                        // specifying both transfer-encoding and content-length is not allowed
                        if(contentLength != null)
                        {
                            throw new BadRequestException();
                        }

                        if(encoding.EndsWith("chunked"))
                        {
                            return new HttpBodyLengthInfo(HttpBodyLengthInfo.LengthType.Chunked);
                        }
                        else
                        {
                            if(!isRequest)
                            {
                                return new HttpBodyLengthInfo(HttpBodyLengthInfo.LengthType.CloseConnection);
                            }
                            else
                            {
                                // request body length cannot be estimated in this case
                                throw new BadRequestException();
                            }
                        }
                    }
                    else if(contentLength != null)
                    {
                        long length = 0;
                        if(contentLength.IndexOf(',') > -1 || !long.TryParse(contentLength, out length))
                        {
                            throw new BadRequestException();
                        }
                        else
                        {
                            return new HttpBodyLengthInfo(length);
                        }
                    }
                    else
                    {
                        if(isRequest)
                        {
                            return new HttpBodyLengthInfo((long)0);
                        }
                        else
                        {
                            return new HttpBodyLengthInfo(HttpBodyLengthInfo.LengthType.CloseConnection);
                        }
                    }
                }
                else
                {
                    throw new ParsingNotFinishedException();
                }
            }
        }
    }
}