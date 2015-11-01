using Netool.Logging;
using System;

namespace Netool.Network.DataFormats
{
    /// <summary>
    /// A helper class for conveniently bulding IDataStream of unknown length using in-memory storage (StreamList) at first
    /// and switching to LoggedFile when length threshold is reached.
    /// </summary>
    public class ThresholdedDataBuilder
    {
        private FileLog log;
        private long threshold;
        private long length = 0;
        private StreamList list = new StreamList();
        private LoggedFileBuilder fileBuilder = null;

        /// <summary>
        /// Get the summary length of all data appended so far
        /// </summary>
        public long Length { get { return length; } }

        /// <summary>
        ///
        /// </summary>
        /// <param name="log"></param>
        /// <param name="lengthThreshold">how long the stream must be to switch to LoggedFile (default 5 Mib)</param>
        /// <exception cref="ArgumentNullException">log</exception>
        /// <exception cref="ArgumentOutOfRangeException">lengthThreshold</exception>
        public ThresholdedDataBuilder(FileLog log, long lengthThreshold = 5*1024*1024)
        {
            if (log == null) throw new ArgumentNullException("log");
            if (lengthThreshold <= 0) throw new ArgumentOutOfRangeException("lengthThreshold must be positive!");
            this.log = log;
            this.threshold = lengthThreshold;
        }

        /// <summary>
        /// Appends data to builded stream
        /// </summary>
        /// <remarks>
        /// With large lengthThreshold it will take a while to create the LoggedFile.
        /// </remarks>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">data</exception>
        public void Append(IDataStream data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (length >= threshold)
            {
                fileBuilder.Append(data);
            }
            else if (length + data.Length >= threshold)
            {
                fileBuilder = new LoggedFileBuilder(log);
                fileBuilder.Append(list);
                fileBuilder.Append(data);
                list = null;
            }
            else
            {
                list.Add((IDataStream)data.Clone());
            }
            length += data.Length;
        }

        /// <summary>
        /// Returns resulting IDataStream, the builder cannot be used anymore after calling this method.
        /// </summary>
        /// <returns></returns>
        public IDataStream Close()
        {
            log = null;
            if (length >= threshold) return fileBuilder.Close();
            return list;
        }
    }
}