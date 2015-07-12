﻿using Netool.Network;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Netool.Logging
{
    public class InvalidFileLogException : Exception { }

    public class FileLog
    {
        /**
         * FileLog format:
         * 8B (long) pointer to data about instance | 8B (long) channel count | channel table
         *
         * channel table format:
         * - FileLog.blockSize bytes (must be a power of 2)
         * - first long (8B) is a pointer to the next table, the rest of the table are pointers to channel info
         *
         * channel info format:
         * 8B (long) pointer to channel data | 8B (long) event count |  event table
         *
         * event table format:
         * - FileLog.blockSize bytes (power of 2)
         * - first 8B is a pointer to next table
         * - other 8B (long) items are pointers to serialized events
         **/
        private Stream stream;
        private BinaryWriter binWriter;
        private BinaryReader binReader;
        private IFormatter formatter = new BinaryFormatter();
        private long channelCount = 0;
        public const int BlockSize = 4096;
        public const int ChannelsPerBlock = BlockSize / sizeof(long) - 1;
        public const int EventsPerBlock = BlockSize / sizeof(long) - 1;
        private string filename;

        // first table
        private long currentChannelTable = 2 * sizeof(long);

        private long currentChannelTableSize = 0;

        public FileLog(string filename)
        {
            this.filename = filename;
            stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            init();
        }

        public void Open()
        {
            if (stream == null)
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                binWriter = new BinaryWriter(stream);
                binReader = new BinaryReader(stream);
            }
        }

        private void init()
        {
            // TODO: add some checks here
            binReader = new BinaryReader(stream);
            binWriter = new BinaryWriter(stream);
            stream.Position = 0;
            if (stream.Length >= BlockSize + 2 * sizeof(long))
            {
                stream.Position += sizeof(long);
                channelCount = binReader.ReadInt64();
                initCurrentChannelTable(channelCount);
            }
            else
            {
                binWriter = new BinaryWriter(stream);
                // pointer to instance data
                binWriter.Write((long)0);
                // channel count
                binWriter.Write((long)0);
                writeNewTable();
            }
        }

        private void initCurrentChannelTable(long count)
        {
            if (count >= (BlockSize / sizeof(long)))
            {
                stream.Position = currentChannelTable;
                currentChannelTable = binReader.ReadInt64();
                if (currentChannelTable == 0 || currentChannelTable > stream.Length)
                {
                    throw new InvalidFileLogException();
                }
                // +1, because first item in a channel table is a pointer to next table
                initCurrentChannelTable(count - (BlockSize / sizeof(long)) + 1);
            }
            else
            {
                currentChannelTableSize = count;
            }
        }

        private void writeNewTable()
        {
            for (long i = 0; i < BlockSize; i += sizeof(long))
            {
                binWriter.Write((long)0);
            }
        }

        /// <summary>
        /// Writes the channelCount and closes the file
        /// </summary>
        public void Close()
        {
            lock (stream)
            {
                stream.Position = sizeof(long);
                binWriter.Write(channelCount);
                binWriter.Close();
                binReader = null;
                binWriter = null;
                stream = null;
            }
        }

        /// <summary>
        /// Initializes channel info for new channel
        /// </summary>
        /// <returns>hint - pointer to the beginnig of the channel info</returns>
        public long AddChannel()
        {
            long hint = 0;
            lock (stream)
            {
                Interlocked.Increment(ref channelCount);
                currentChannelTableSize++;
                // current channel table is full
                if (currentChannelTableSize == (BlockSize / sizeof(long)))
                {
                    stream.Position = currentChannelTable;
                    binWriter.Write(stream.Length);
                    stream.Position = stream.Length;
                    currentChannelTable = stream.Position;
                    writeNewTable();
                    currentChannelTableSize = 1;
                }
                stream.Position = currentChannelTable + currentChannelTableSize * sizeof(long);
                // start of the channel info
                hint = stream.Length;
                // write the pointer to channel info to proper field in the channel table
                binWriter.Write(stream.Length);
                // initialize channel info
                stream.Position = stream.Length;
                // pointer to channel data
                binWriter.Write((long)0);
                // eventCount
                binWriter.Write((long)0);
                // event table
                writeNewTable();
            }
            return hint;
        }

        /// <summary>
        /// Write channel data, usually upon closing the channel. Note: don't call this method repeatedly, because there is no way to delete the old data!
        /// </summary>
        /// <param name="hint">pointer to channel info as returned from AddChannel</param>
        /// <param name="eventCount">channel's eventCount</param>
        /// <param name="channel"></param>
        public void WriteChannelData(long hint, int eventCount, IChannel channel)
        {
            lock (stream)
            {
                stream.Position = hint;
                // write the pointer to serialized channel data
                binWriter.Write(stream.Length);
                binWriter.Write((long)eventCount);
                stream.Position = stream.Length;
                formatter.Serialize(stream, channel);
            }
        }

        /// <summary>
        /// Write instance data, usually upon closing the instance. Note: don't call this method repeatedly, because there is no way to delete the old data!
        /// </summary>
        /// <param name="instance"></param>
        public void WriteInstanceData(IInstance instance)
        {
            lock (stream)
            {
                stream.Position = 0;
                // write the pointer to serialized instance data
                binWriter.Write(stream.Length);
                stream.Position = stream.Length;
                formatter.Serialize(stream, instance);
            }
        }

        /// <summary>
        /// Logs event
        /// </summary>
        /// <param name="hint">a pointer to the begging of the channel info, as returned by AddChannel method</param>
        /// <param name="e">event to log</param>
        public void LogEvent(long hint, Event e)
        {
            lock (stream)
            {
                // move the hint to the beginning of the event table
                hint += 2*sizeof(long);
                logEventHelper(hint, e.ID, e);
            }
        }

        private void logEventHelper(long eventTable, long remainingOffset, Event e)
        {
            // < because the first item is a pointer to next table
            if (remainingOffset <= EventsPerBlock)
            {
                stream.Position = eventTable + remainingOffset * sizeof(long);
                long eventPos = stream.Length;
                binWriter.Write(eventPos);
                stream.Position = stream.Length;
                formatter.Serialize(stream, e);
            }
            // event doesn't belong to the current table
            else
            {
                // get the adress of the next event table
                stream.Position = eventTable;
                long next = binReader.ReadInt64();
                // this is the last table -> create a new one
                if (next == 0)
                {
                    // set the next table pointer
                    stream.Position = eventTable;
                    binWriter.Write(stream.Length);
                    next = stream.Length;
                    stream.Position = next;
                    writeNewTable();
                }
                // -1 for the next table pointer
                remainingOffset -= EventsPerBlock;
                logEventHelper(next, remainingOffset, e);
            }
        }

        public int GetChannelCount()
        {
            return (int)channelCount;
        }

        public FileLogReader CreateReader()
        {
            return new FileLogReader(filename, this);
        }
    }
}