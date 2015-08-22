using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Netool.Network.DataFormats.Http
{
    [Serializable]
    public class ChunkedDecoderException : Exception { }

    [Serializable]
    public class InvalidChunkException : ChunkedDecoderException { }

    [Serializable]
    public class PartialChunkException : ChunkedDecoderException { }

    public class ChunkedDecoder
    {
        public struct DecodeOneInfo
        {
            public readonly ByteArray Data;
            public readonly long BytesRead;

            public DecodeOneInfo(ByteArray data, long bytesRead)
            {
                Data = data;
                BytesRead = bytesRead;
            }
        }

        public struct DecodeInfo
        {
            public readonly IDataStream DecodedData;
            public readonly bool Finished;

            public DecodeInfo(IDataStream data, bool finished = false)
            {
                DecodedData = data;
                Finished = finished;
            }
        }

        private static string sizePt = @"";
        private static Regex chunkHeaderRegex = new Regex(@"^(?<Size>[0-9A-Fa-f]+)");
        private static int maxChunkExtSize = 8192;

        private StreamList input = new StreamList();

        public DecodeInfo Decode(IDataStream data)
        {
            var output = new StreamList();
            input.Add(data);
            var segment = new StreamSegment(input);
            try
            {
                while (true)
                {
                    var chunkInfo = ChunkedDecoder.DecodeOneChunk(segment);
                    // last chunk
                    if (chunkInfo.Data.Length == 0)
                    {
                        return new DecodeInfo(output, true);
                    }
                    else
                    {
                        output.Add(chunkInfo.Data);
                        segment = new StreamSegment(segment.Stream, segment.Offset + chunkInfo.BytesRead);
                        // no more data to read from current packet
                        if (segment.Length == 0) break;
                    }
                }
            }
            catch (PartialChunkException) { }
            finally
            {
                if (segment.Length > 0)
                {
                    if(segment.Length < input.Length)
                    {
                        var d = new ByteArray(segment);
                        input = new StreamList();
                        input.Add(d);
                    }
                }
                else
                {
                    input = new StreamList();
                }
            }
            return new DecodeInfo(output, false);
        }

        public static DecodeOneInfo DecodeOneChunk(IDataStream stream)
        {
            if (stream == null) throw new ArgumentNullException();

            long len = Math.Min(stream.Length, 10);
            var buff = stream.ReadBytes(0, len);
            var str = ASCIIEncoding.ASCII.GetString(buff);
            var match = chunkHeaderRegex.Match(str);

            if (match.Success)
            {
                try
                {
                    var size = Convert.ToInt64(match.Groups["Size"].Value, 16);
                    if (size < 0)
                    {
                        throw new InvalidChunkException();
                    }
                    // last chunk
                    if(size == 0)
                    {
                        // read trailer part - ignore for now
                        len = match.Length;
                        long endIndex = -1;
                        while (len < stream.Length)
                        {
                            var len2 = Math.Min(stream.Length - len, 512);
                            buff = stream.ReadBytes(len, len2);
                            str = ASCIIEncoding.ASCII.GetString(buff);
                            // find end
                            endIndex = (int) str.IndexOf("\r\n\r\n");
                            if(endIndex > 0)
                            {
                                len += endIndex + 4;
                                break;
                            }
                            len += len2;
                        }
                        if(endIndex < 0)
                        {
                            throw new PartialChunkException();
                        }
                        return new DecodeOneInfo(new ByteArray(new byte[0]), len);
                    }
                    buff = stream.ReadBytes(match.Length, Math.Min(stream.Length - match.Length, size + maxChunkExtSize));
                    str = ASCIIEncoding.ASCII.GetString(buff);
                    // start of chunk
                    len = str.IndexOf("\r\n") + 2;
                    if(len < 2)
                    {
                        throw new PartialChunkException();
                    }
                    ByteArray ret;
                    if(buff.Length >= (len + size + 2))
                    {
                        if (str.Substring((int) (len + size), 2) != "\r\n") throw new InvalidChunkException();
                        ret = new ByteArray(buff, (int)len, (int)size);
                    }
                    else if(stream.Length < match.Length + len + size + 2)
                    {
                        throw new PartialChunkException();
                    }
                    else
                    {
                        var crlf = stream.ReadBytes(match.Length + len + size, 2);
                        if (crlf[0] != 13 || crlf[1] != 10) throw new InvalidChunkException();
                        ret = new ByteArray(stream, len, size);
                    }

                    // +2 for final CRLF
                    return new DecodeOneInfo(ret, match.Length + len + size + 2);
                }
                catch(ChunkedDecoderException e)
                {
                    throw;
                }
                catch
                {
                    throw new InvalidChunkException();
                }
            }
            else
            {
                // unable to parse chunk size
                throw new InvalidChunkException();
            }
        }
    }
}