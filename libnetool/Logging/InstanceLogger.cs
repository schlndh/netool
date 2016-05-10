using Netool.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Netool.Logging
{
    public class InstanceLogger : IDisposable
    {
        private ConcurrentDictionary<int, ChannelLogger> channelsInfo = new ConcurrentDictionary<int, ChannelLogger>();
        private object channelsLock = new object();
        private List<IChannel> channels = new List<IChannel>();
        private FileLog log;
        public bool IsTempFile { get; private set; }
        public string Filename { get { return log.Filename; } }

        public event EventHandler<int> ChannelCountChanged;

        /// <summary>
        /// True if log file has no channels or logged files.
        /// </summary>
        public bool IsEmpty { get { return log.GetChannelCount() == 0 && log.GetFileCount() == 0; } }

        /// <summary>
        /// Constructs instance logger with temporary file
        /// </summary>
        public InstanceLogger()
        {
            log = new FileLog(Path.GetTempFileName());
            IsTempFile = true;
        }

        /// <summary>
        /// Constructs InstanceLogger with given file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mode"></param>
        public InstanceLogger(string filename, FileMode mode = FileMode.OpenOrCreate)
        {
            log = new FileLog(filename, mode);
            IsTempFile = false;
            int count = log.GetChannelCount();
            using (var reader = log.ReaderPool.Get())
            {
                channels.AddRange(reader.ReadChannelsData(1, count));
            }
        }

        public void AddChannel(IChannel channel)
        {
            int c = 0;
            lock (channelsLock)
            {
                var hint = log.AddChannel();
                channels.Add(channel);
                c = channels.Count;
                channelsInfo.TryAdd(channel.ID, new ChannelLogger(log, hint, channel));
            }
            OnChannelCountChanged(c);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">1-based ID</param>
        /// <returns></returns>
        public ChannelLogger GetChannelLogger(int id)
        {
            ChannelLogger logger;
            channelsInfo.TryGetValue(id, out logger);
            if(logger == null)
            {
                using (var reader = log.ReaderPool.Get())
                {
                    var channel = GetChannelByID(id);
                    var hint = reader.GetChannelInfoHintByID(id);
                    var eventCount = reader.GetEventCount(hint);
                    logger = new ChannelLogger(log, hint, channel, eventCount);
                }
            }
            return logger;
        }

        /// <summary>
        /// Call this when the instance is started
        /// </summary>
        public void Open()
        {
            log.Open();
        }

        /// <inheritdoc cref="FileLog.WriteInstanceData"/>
        public void WriteInstanceData(IInstance instance)
        {
            Debug.WriteLine("InstanceLogger - writing instance data (type: {0})", instance.GetType());
            log.WriteInstanceData(instance);
        }

        /// <inheritdoc cref="FileLogReader.ReadInstanceData"/>
        public IInstance ReadInstanceData()
        {
            using (var reader = log.ReaderPool.Get())
            {
                var ret = reader.ReadInstanceData();
                return ret;
            }
        }

        /// <inheritdoc cref="FileLog.WritePluginID"/>
        public void WritePluginID(long id)
        {
            log.WritePluginID(id);
        }

        /// <inheritdoc cref="FileLogReader.ReadPluginID"/>
        public long ReadPluginID()
        {
            using (var reader = log.ReaderPool.Get())
            {
                var id = reader.ReadPluginID();
                return id;
            }
        }

        /// <inheritdoc cref="FileLog.WriteInstanceName"/>
        public void WriteInstanceName(string name)
        {
            log.WriteInstanceName(name);
        }

        /// <inheritdoc cref="FileLogReader.ReadInstanceName"/>
        public string ReadInstanceName()
        {
            using (var reader = log.ReaderPool.Get())
            {
                var name = reader.ReadInstanceName();
                return name;
            }
        }

        /// <inheritdoc cref="FileLog.MoveToFile"/>
        public void MoveToFile(string target)
        {
            log.MoveToFile(target);
        }

        /// <inheritdoc cref="FileLog.DeleteFile"/>
        public void DeleteFile()
        {
            log.DeleteFile();
        }

        /// <summary>
        /// Call this when the instance is closed
        /// </summary>
        public void Close()
        {
            Debug.WriteLine("InstanceLogger - closing");
            log.Close();
        }

        private void OnChannelCountChanged(int c)
        {
            var ev = ChannelCountChanged;
            if (ev != null) ev(this, c);
        }

        public int GetChannelCount()
        {
            lock (channelsLock)
            {
                return channels.Count;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">1-based ID</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public IChannel GetChannelByID(int id)
        {
            lock(channelsLock)
            {
                if (id < 1 || id > GetChannelCount()) throw new ArgumentOutOfRangeException("id");
                return channels[id - 1];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="firstID">1-based ID</param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">firstID, count</exception>
        public IList<IChannel> GetChannelRange(int firstID, int count)
        {
            lock(channelsLock)
            {
                if (firstID < 1 || firstID - 1 + count > GetChannelCount()) throw new ArgumentOutOfRangeException("firstID, count");
                return new List<IChannel>(channels.Skip(firstID - 1).Take(count));
            }
        }

        public LoggedFileBuilder CreateFileBuilder()
        {
            return new LoggedFileBuilder(log);
        }

        /// <inheritdoc cref="FileLog.GetFileCount"/>
        public long GetFileCount()
        {
            return log.GetFileCount();
        }

        public void Dispose()
        {
            ((IDisposable)log).Dispose();
        }
    }
}