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

        private static Regex chunkHeaderRegex = new Regex(@"^(?<Size>[0-9A-Fa-f]+)(?<ChunkExt>;[^\r\n]*)?(?<HeaderEnd>\r\n)?");

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

        private static long findFirst(IDataStream stream, string needle, long offset)
        {
            long endIndex = -1;
            while (offset < stream.Length)
            {
                var len2 = Math.Min(stream.Length - offset, 512);
                var buff = stream.ReadBytes(offset, len2);
                var str = ASCIIEncoding.ASCII.GetString(buff);
                // find end
                endIndex = (int)str.IndexOf(needle);
                if (endIndex > -1)
                {
                    offset += endIndex + needle.Length;
                    return offset;
                }
                offset += len2;
            }
            if (endIndex < 0)
            {
                throw new PartialChunkException();
            }
            return -1;
        }

        /// <summary>
        /// Decodes one chunk from stream (chunk must begin at the beggining of the stream)
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">stream is null</exception>
        /// <exception cref="InvalidChunkException">chunk is invalid</exception>
        /// <exception cref="PartialChunkException">not enough data in the stream to decode the whole chunk</exception>
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
                    if (!match.Groups["ChunkExt"].Success && !match.Groups["HeaderEnd"].Success)
                    {
                        if (len == stream.Length) throw new PartialChunkException();
                        // chunk size too large
                        else throw new InvalidChunkException();
                    }
                    // parse next chunk size
                    var size = Convert.ToInt64(match.Groups["Size"].Value, 16);
                    if (size < 0)
                    {
                        throw new InvalidChunkException();
                    }
                    // locate \r\n terminating the chunk header
                    if(match.Groups["HeaderEnd"].Success)
                    {
                        len = match.Length;
                    }
                    else
                    {
                        len = findFirst(stream, "\r\n", len);
                    }
                    // len points to the beggining of chunk data

                    // last chunk
                    if(size == 0)
                    {
                        // rewind len back to include \r\n again, in case there is no trailer
                        len -= 2;
                        // skip over the trailer part
                        len = findFirst(stream, "\r\n\r\n", len);
                        return new DecodeOneInfo(new ByteArray(new byte[0]), len);
                    }
                    // size > 0
                    // not enough data
                    if (stream.Length - len < size + 2) throw new PartialChunkException();
                    // check that chunk data is ended with CRLF
                    buff = stream.ReadBytes(len + size, 2);
                    if (buff[0] != 13 || buff[1] != 10) throw new InvalidChunkException();

                    ByteArray ret = new ByteArray(stream, len, size);
                    return new DecodeOneInfo(ret, len + size + 2);
                }
                catch(ChunkedDecoderException)
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