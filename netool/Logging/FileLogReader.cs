using Netool.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Netool.Logging
{
    public class FileLogReader
    {
        private Stream stream;
        private BinaryReader binReader;
        private BinaryFormatter formatter = new BinaryFormatter();
        private FileLog log;

        public FileLogReader(string filename, FileLog log)
        {
            stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            binReader = new BinaryReader(stream);
            this.log = log;
        }

        public void Close()
        {
            binReader.Close();
        }

        /// <summary>
        /// Reads instance data
        /// </summary>
        /// <returns>IInstance or null if instance data have not yet been written</returns>
        public IInstance ReadInstanceData()
        {
            // TODO: reading can run in parallel with writing
            lock (stream)
            {
                stream.Position = 0;
                var dataPtr = binReader.ReadInt64();
                if (dataPtr != 0)
                {
                    stream.Position = dataPtr;
                    // TODO: error handling
                    return (IInstance)formatter.Deserialize(stream);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads multiple consecutive channels at onec
        /// </summary>
        /// <param name="firstID"></param>
        /// <param name="count"></param>
        /// <returns>chanells in a list</returns>
        public List<IChannel> ReadChannelsData(int firstID, int count)
        {
            var ret = new List<IChannel>(count);
            int off;
            var table = getChannelTableHintByID(firstID, out off);
            lock (stream)
            {
                while(count > 0)
                {
                    while(count > 0 && off < FileLog.ChannelsPerBlock)
                    {
                        stream.Position = table + off * sizeof(long);
                        ret.Add(readChannelDataByHint(binReader.ReadInt64()));
                        off++;
                        count--;
                    }
                    stream.Position = table;
                    // next table
                    stream.Position = binReader.ReadInt64();
                    off = 1; // 0 is a pointer to next table
                }
            }
            return ret;
        }

        /// <summary>
        /// Reads channel data
        /// </summary>
        /// <param name="id">channel id</param>
        /// <returns>channel</returns>
        public IChannel ReadChannelData(int id)
        {
            if(log.GetChannelCount() >= id)
            {
                int off;
                var hint = getChannelTableHintByID(id, out off);

                lock (stream)
                {
                    // move stream position to the correct channel pointer
                    stream.Position += off * sizeof(long);
                    // move the stream position to channel data
                    return readChannelDataByHint(binReader.ReadInt64());
                }
            }
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hint">pointer to channel info structure</param>
        /// <returns>channel data</returns>
        private IChannel readChannelDataByHint(long hint)
        {
            // no locking here, must be called inside a lock statement
            stream.Position = hint;
            // read the pointer to channel data
            stream.Position = binReader.ReadInt64();
            // TODO: error handling
            return (IChannel)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Get a pointer to a channel table containing specific channel
        /// </summary>
        /// <param name="id">the id of the channel, must be correct</param>
        /// <param name="offset">the channel offset in returned table (it must be multiplied by sizeof(long) to get the actual byte offset)</param>
        /// <returns>a pointer to channel table</returns>
        private long getChannelTableHintByID(int id, out int offset)
        {
            lock(stream)
            {
                // start of the first channel table
                stream.Position = 2 * sizeof(long);
                var off = id;
                // move stream position to the correct channel table
                while (off > FileLog.ChannelsPerBlock)
                {
                    // TODO: error handling here
                    // next table
                    stream.Position = binReader.ReadInt64();
                    off -= FileLog.ChannelsPerBlock;
                }
                offset = off;
                return stream.Position;
            }
        }

        /// <summary>
        /// Get a pointer to channel info structure by channel id
        /// </summary>
        /// <param name="id">channel id</param>
        /// <returns>hint - a pointer to channel info structure</returns>
        public long GetChannelInfoHintByID(int id)
        {
            lock(stream)
            {
                int off;
                var table = getChannelTableHintByID(id, out off);
                // read a channel table entry - contains a pointer to channel info
                stream.Position = table + off * sizeof(long);
                return binReader.ReadInt64();
            }
        }

        /// <summary>
        /// Get channels event count
        /// </summary>
        /// <param name="hint">a pointer to correct channelInfo structure</param>
        /// <returns>channel's event count</returns>
        public int GetEventCount(long hint)
        {
            lock(stream)
            {
                // first (long) is a pointer to channel data
                stream.Position = hint + sizeof(long);
                var ret = binReader.ReadInt64();
                return (int)ret;
            }
        }

        /// <summary>
        /// Reads Event from log
        /// </summary>
        /// <param name="hint">a pointer to channel info structure</param>
        /// <param name="id">Event ID</param>
        /// <returns>Event</returns>
        public Event ReadEvent(long hint, int id)
        {
            lock(stream)
            {
                int off;
                // + 2*sizeof(long) to skip the pointer to channel data and event count
                var table = getEventTableByIndex(hint + 2 * sizeof(long), id, out off);
                stream.Position = table + off * sizeof(long);
                return readEventByPointer(binReader.ReadInt64());
            }
        }

        /// <summary>
        /// Reads several consecutive events - more efficient than reading one by one using ReadEvent
        /// </summary>
        /// <param name="hint">channel info hint</param>
        /// <param name="firstID">ID of the first required event</param>
        /// <param name="count">number of consecutive events to return</param>
        /// <returns></returns>
        public List<Event> ReadEvents(long hint, int firstID, int count)
        {
            var ret = new List<Event>(count);
            int off;
            var table = getEventTableByIndex(hint + 2*sizeof(long), firstID, out off);
            lock (stream)
            {
                while (count > 0)
                {
                    while (count > 0 && off < FileLog.EventsPerBlock)
                    {
                        stream.Position = table + off * sizeof(long);
                        ret.Add(readEventByPointer(binReader.ReadInt64()));
                        off++;
                        count--;
                    }
                    stream.Position = table;
                    // next table
                    stream.Position = binReader.ReadInt64();
                    off = 1; // 0 is a pointer to next table
                }
            }
            return ret;
        }

        /// <summary>
        /// Finds the event table containing event with given index
        /// </summary>
        /// <param name="eventTable">current event table</param>
        /// <param name="index">remaining index</param>
        /// <param name="offset">offset of the event in the returned table (must be multiplied by sizeof(long) to get the actual byte offset)</param>
        /// <returns>event table containing the event</returns>
        private long getEventTableByIndex(long eventTable, int index, out int offset)
        {
            if(index > FileLog.EventsPerBlock)
            {
                // no locking, will be called from inside of a lock statement
                stream.Position = eventTable;
                return getEventTableByIndex(binReader.ReadInt64(), index - FileLog.EventsPerBlock, out offset);
            }
            else
            {
                offset = index;
                return eventTable;
            }
        }

        private Event readEventByPointer(long ptr)
        {
            // no locking, lock outside
            stream.Position = ptr;
            return (Event)formatter.Deserialize(stream);
        }
    }
}