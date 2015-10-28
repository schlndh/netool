﻿using Netool.Network;
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
        /// <summary>
        /// Context passed to deserialized objects
        /// </summary>
        public class DeserializationContext
        {
            public readonly FileLog Log;

            public DeserializationContext(FileLog log)
            {
                Log = log;
            }
        }
        private Stream stream;
        private readonly object streamLock = new object();
        private BinaryReader binReader;
        private BinaryFormatter formatter;
        private FileLog log;
        private long fileTable = 0;

        public FileLogReader(string filename, FileLog log)
        {
            stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            binReader = new BinaryReader(stream);
            this.log = log;
            formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All, new DeserializationContext(log)));
        }

        public void Close()
        {
            binReader.Close();
        }

        /// <summary>
        /// Reads Plugin ID
        /// </summary>
        /// <returns>Plugin ID</returns>
        public long ReadPluginID()
        {
            lock (streamLock)
            {
                stream.Position = 0;
                // move to format info structure - plugin ID field
                stream.Position = binReader.ReadInt64() + 2 * sizeof(long);
                return binReader.ReadInt64();
            }
        }

        /// <summary>
        /// Reads Instance Name
        /// </summary>
        /// <returns>instance name or null if it wasn't written</returns>
        public string ReadInstanceName()
        {
            lock (streamLock)
            {
                stream.Position = 0;
                // move to format info structure - pointer to instance name field
                stream.Position = binReader.ReadInt64() + 3 * sizeof(long);
                var pos =  binReader.ReadInt64();
                // instance name wasn't written
                if (pos == 0)
                {
                    return null;
                }
                else
                {
                    stream.Position = pos;
                    return (string)formatter.Deserialize(stream);
                }
            }
        }
        /// <summary>
        /// Reads instance data
        /// </summary>
        /// <returns>IInstance or null if instance data have not yet been written</returns>
        public IInstance ReadInstanceData()
        {
            lock (streamLock)
            {
                stream.Position = 0;
                // format info structure - instance data field
                stream.Position = binReader.ReadInt64() + sizeof(long);
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
        /// Reads multiple consecutive channels at once.
        /// Note that all required channel data must already be written, this method doesn't check that.
        /// </summary>
        /// <param name="firstID">1-based channel id</param>
        /// <param name="count"></param>
        /// <returns>chanells in a list</returns>
        public List<IChannel> ReadChannelsData(int firstID, int count)
        {
            var ret = new List<IChannel>(count);
            int off;
            var table = getChannelTableHintByID(firstID, out off);
            lock (streamLock)
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
        /// <param name="id">1-based channel id</param>
        /// <returns>channel</returns>
        public IChannel ReadChannelData(int id)
        {
            if(log.GetChannelCount() >= id)
            {
                int off;
                var hint = getChannelTableHintByID(id, out off);

                lock (streamLock)
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
            lock (streamLock)
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
        /// <param name="id">1-based channel id</param>
        /// <returns>hint - a pointer to channel info structure</returns>
        public long GetChannelInfoHintByID(int id)
        {
            lock (streamLock)
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
            lock (streamLock)
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
        /// <param name="id">1-based Event ID</param>
        /// <returns>Event</returns>
        public Event ReadEvent(long hint, int id)
        {
            lock (streamLock)
            {
                int off;
                // + 2*sizeof(long) to skip the pointer to channel data and event count
                var table = getEventTableByIndex(hint + 2 * sizeof(long), id, out off);
                stream.Position = table + off * sizeof(long);
                return readEventByPointer(binReader.ReadInt64());
            }
        }

        /// <summary>
        /// Reads several consecutive events - more efficient than reading one by one using ReadEvent.
        /// Note that all required events must already be written, this method doesn't check that.
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
            lock (streamLock)
            {
                while (count > 0)
                {
                    while (count > 0 && off <= FileLog.EventsPerBlock)
                    {
                        stream.Position = table + off * sizeof(long);
                        ret.Add(readEventByPointer(binReader.ReadInt64()));
                        off++;
                        count--;
                    }
                    stream.Position = table;
                    // next table
                    table = binReader.ReadInt64();
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

        /// <summary>
        /// Get file hint by file ID
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns>a hint which can be used to access data from file. Returns 0 if ID is invalid.</returns>
        public long GetFileHint(long fileID)
        {
            lock(streamLock)
            {
                if(fileTable == 0)
                {
                    stream.Position = 0;
                    // move to format info - Ptr to file table field
                    stream.Position = binReader.ReadInt64() + 4 * sizeof(long);
                    fileTable = binReader.ReadInt64();
                    if (fileTable == 0)
                    {
                        return 0;
                    }
                }
                stream.Position = fileTable;
                long currentFBT = fileTable;
                // locate the correct FBT
                while(fileID > FileLog.FilesPerBlock)
                {
                    fileID -= FileLog.FilesPerBlock;
                    currentFBT = binReader.ReadInt64();
                    if(currentFBT == 0)
                    {
                        // file not found
                        return 0;
                    }
                    stream.Position = currentFBT;
                }
                stream.Position += fileID * sizeof(long);
                return binReader.ReadInt64();
            }
        }

        /// <summary>
        /// Get file lentgh
        /// </summary>
        /// <param name="hint">file hint as returned from GetFileHint</param>
        /// <returns>file length</returns>
        public long GetFileLength(long hint)
        {
            lock(streamLock)
            {
                stream.Position = hint;
                return binReader.ReadInt64();
            }
        }

        /// <summary>
        /// Read file data to buffer
        /// </summary>
        /// <param name="hint">file hint as returned from GetFileHint</param>
        /// <param name="buffer">output buffer</param>
        /// <param name="start">file offset to start reading at</param>
        /// <param name="length">how many data to read</param>
        /// <param name="offset">start offset in the output buffer</param>
        /// <exception cref="ArgumentNullException">Buffer is null</exception>
        /// <exception cref="BufferNotLargeEnoughException"></exception>
        /// <exception cref="IndexOutOfRangeException">Data outside the file requested.</exception>
        /// <exception cref="LoggedFileCorruptedException">The logged file is somehow corrupted - eg. invalid pointers, missing data, ...</exception>
        public void ReadFileDataToBuffer(long hint, byte[] buffer, long start, int length, int offset)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length - offset < length) throw new BufferNotLargeEnoughException();
            lock (streamLock)
            {
                stream.Position = hint;
                long size = binReader.ReadInt64();
                if(size < start + length)
                {
                    throw new IndexOutOfRangeException("Data outside the file requested.");
                }
                long blockOffset = start % FileLog.BlockSize;
                long fbt2Offset = (start / (FileLog.BlockSize)) % (FileLog.FilesPerBlock + 1);
                long fbtOffset = start / (FileLog.BlockSize * (FileLog.FilesPerBlock + 1)) + 1;
                long currentFbt = stream.Position;
                long currentFbt2 = -1;
                while(length > 0)
                {
                    while(fbtOffset <= FileLog.FilesPerBlock)
                    {
                        while(fbt2Offset <= FileLog.FilesPerBlock)
                        {
                            if(currentFbt2 < 0)
                            {
                                stream.Position = currentFbt + fbtOffset * sizeof(long);
                                currentFbt2 = binReader.ReadInt64();
                                if(currentFbt2 <=0 || currentFbt2 > stream.Length - FileLog.BlockSize) throw new LoggedFileCorruptedException(string.Format("Expected valid pointer to FBT2 at {0}! File hint: {1}.", stream.Position - sizeof(long), hint));
                            }
                            while(blockOffset < FileLog.BlockSize)
                            {
                                stream.Position = currentFbt2 + fbt2Offset * sizeof(long);
                                long dataBlock = binReader.ReadInt64();
                                if (dataBlock <= 0 || dataBlock > stream.Length - FileLog.BlockSize) throw new LoggedFileCorruptedException(string.Format("Expected valid pointer to data block at {0}! File hint: {1}.", stream.Position - sizeof(long), hint));
                                stream.Position = dataBlock + blockOffset;
                                int toRead = Math.Min(length,(int) (FileLog.BlockSize - blockOffset));
                                int read = 0;
                                // although all data should be available the implementation may not return them all at once
                                while (toRead > 0 && (read = stream.Read(buffer, offset, toRead)) != 0)
                                {
                                    toRead -= read;
                                    offset += read;
                                    length -= read;
                                    blockOffset += read;
                                }
                                if(toRead > 0) throw new LoggedFileCorruptedException(string.Format("Expected to read {0} more bytes from data block starting at {1}. File hint: {2}.", toRead, dataBlock, hint));
                                if (length == 0) return;
                            }
                            blockOffset = 0;
                            fbt2Offset++;
                        }
                        fbt2Offset = 0;
                        fbtOffset++;
                    }
                    stream.Position = currentFbt;
                    currentFbt = binReader.ReadInt64();
                    if (currentFbt <= 0 || currentFbt > stream.Length - FileLog.BlockSize) throw new LoggedFileCorruptedException(string.Format("Expected valid pointer to next FBT at {0}! File hint: {1}.", stream.Position - sizeof(long), hint));
                    stream.Position = currentFbt;
                    fbtOffset -= FileLog.FilesPerBlock;
                    currentFbt2 = -1;
                }
            }
        }
    }
}