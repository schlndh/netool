using Netool.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Netool.Logging
{
    public class InstanceLogger
    {
        private ConcurrentDictionary<int, ChannelLogger> channelsInfo = new ConcurrentDictionary<int, ChannelLogger>();
        private LinkedList<IChannel> channels = new LinkedList<IChannel>();
        private FileLog log;
        public bool IsTempFile { get; private set; }

        public event EventHandler<int> ChannelCountChanged;

        public InstanceLogger()
        {
            log = new FileLog(Path.GetTempFileName());
            IsTempFile = true;
        }

        public InstanceLogger(string filename, FileMode mode = FileMode.OpenOrCreate)
        {
            log = new FileLog(filename, mode);
            IsTempFile = false;
            // temporary workaround - load all channels
            GetChannelByID(log.GetChannelCount());
        }

        public void AddChannel(IChannel channel)
        {
            var hint = log.AddChannel();
            int c = 0;
            lock(channels)
            {
                channels.AddLast(channel);
                c = channels.Count;
            }
            channelsInfo.TryAdd(channel.ID, new ChannelLogger(log, hint, channel));
            OnChannelCountChanged(c);
        }

        public ChannelLogger GetChannelLogger(int id)
        {
            ChannelLogger logger;
            channelsInfo.TryGetValue(id, out logger);
            if(logger == null)
            {
                var reader = log.CreateReader();
                var channel = GetChannelByID(id).Value;
                var hint = reader.GetChannelInfoHintByID(id);
                var eventCount = reader.GetEventCount(hint);
                logger = new ChannelLogger(log, hint, channel, eventCount);
                reader.Close();
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
            var reader = log.CreateReader();
            var ret = reader.ReadInstanceData();
            reader.Close();
            return ret;
        }

        /// <inheritdoc cref="FileLog.WritePluginID"/>
        public void WritePluginID(long id)
        {
            log.WritePluginID(id);
        }

        /// <inheritdoc cref="FileLogReader.ReadPluginID"/>
        public long ReadPluginID()
        {
            var reader = log.CreateReader();
            var id = reader.ReadPluginID();
            reader.Close();
            return id;
        }

        /// <inheritdoc cref="FileLog.WriteInstanceName"/>
        public void WriteInstanceName(string name)
        {
            log.WriteInstanceName(name);
        }

        /// <inheritdoc cref="FileLogReader.ReadInstanceName"/>
        public string ReadInstanceName()
        {
            var reader = log.CreateReader();
            var name = reader.ReadInstanceName();
            reader.Close();
            return name;
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
            if (ChannelCountChanged != null) ChannelCountChanged(this, c);
        }

        /// <inheritdoc cref="FileLog.GetChannelCount"/>
        public int GetChannelCount()
        {
            return log.GetChannelCount();
        }

        public LinkedListNode<IChannel> GetChannelByID(int id)
        {
            if(channels.Count < id)
            {
                var reader = log.CreateReader();
                // read all channels between the last already read and the one requested
                var missing = reader.ReadChannelsData(channels.Count + 1, id - channels.Count);
                lock(channels)
                {
                    foreach(var item in missing)
                    {
                        channels.AddLast(item);
                    }
                }
                reader.Close();
            }
            var curr = channels.First;
            while (id-- > 1)
            {
                curr = curr.Next;
            }
            return curr;
        }

        public LoggedFileBuilder CreateFileBuilder()
        {
            return new LoggedFileBuilder(log);
        }
    }
}