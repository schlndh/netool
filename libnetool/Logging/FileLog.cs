using Netool.Network;
using Netool.Network.DataFormats;
using System;
using System.Diagnostics;
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
         * Terminology:
         * Format info: a structure that holds information about the logging format
         * Channel info: a structure that holds information about a channel and its events
         * Channel data: serialized channel
         * File: data stored separately from an event, all metadata (except for file size) are stored in the event (or as another file if necessary)
         *  - files can be arbitrarily large (limited by file-system and long value range), however locating a data block requires approximatelly N + 1 steps when N is the number of gigabytes stored in the file
         *
         * FileLog format:
         * 8B (long) pointer to format info | 8B (long) channel count | channel table
         *
         * Format info format:
         * 8B (long) format version | 8B (long) pointer to instance data | 8B (long) ProtocolPlugin ID | 8B (long) pointer to InstanceName
         * 8B (long) pointer to file table | 8B (long) file count
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
         *
         * file table format:
         * - FileLog.blockSize bytes (must be a power of 2)
         * - first long (8B) is a pointer to the next table, the rest of the table are pointers to files
         *
         * file format:
         * 8B (long) file size | file block table (level 1)
         *
         * file block table (1) format:
         * - FileLog.blockSize bytes (must be a power of 2)
         * - first long (8B) is a pointer to the next table, the rest of the table are pointers to FBT level 2
         *
         * file block table (2) format:
         * - FileLog.blockSize bytes (must be a power of 2)
         * 8B (long) pointers to data blocks (FileLog.blockSize bytes)
         **/
        private FileStream stream;
        private readonly object streamLock = new object();
        private BinaryWriter binWriter;
        private BinaryReader binReader;
        private IFormatter formatter = new BinaryFormatter();
        private long channelCount = 0;
        private long fileCount = 0;
        private long currentFileTable = 0;
        private long currentFileTableSize = 0;
        private byte[] blockBuffer = null;
        /// <summary>
        /// A constant indicating current version of FileLog format, will be incremented each time a format is changed
        /// </summary>
        public const long FormatVersion = 1;
        /// <summary>
        /// Size of 1 block in bytes
        /// </summary>
        public const int BlockSize = 4096;
        /// <summary>
        /// The number of actual channel info pointers in 1 block (not counting a pointer to the next table)
        /// </summary>
        public const int ChannelsPerBlock = BlockSize / sizeof(long) - 1;
        /// <summary>
        /// The number of actual event pointers in 1 block (not counting a pointer to the next table)
        /// </summary>
        public const int EventsPerBlock = BlockSize / sizeof(long) - 1;
        /// <summary>
        /// The number of actual file pointers in 1 block (not counting a pointer to the next table)
        /// </summary>
        public const int FilesPerBlock = BlockSize / sizeof(long) - 1;
        private string filename;

        // first table
        private long currentChannelTable = 2 * sizeof(long);

        private long currentChannelTableSize = 0;

        public FileLog(string filename, FileMode mode = FileMode.OpenOrCreate)
        {
            this.filename = filename;
            stream = new FileStream(filename, mode, FileAccess.ReadWrite, FileShare.Read);
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
                stream.Position = 0;
                // move to format info - file count field
                stream.Position = binReader.ReadInt64() + 5 * sizeof(long);
                fileCount = binReader.ReadInt64();
            }
            else
            {
                binWriter = new BinaryWriter(stream);
                stream.Position = sizeof(long);
                // channel count
                binWriter.Write((long)0);
                writeNewTable();
                // write a pointer to format info
                stream.Position = 0;
                binWriter.Write(stream.Length);
                stream.Position = stream.Length;
                binWriter.Write(FormatVersion);
                // a pointer to instance data
                binWriter.Write((long)0);
                // Plugin ID
                binWriter.Write((long)0);
                // a pointer to instance name
                binWriter.Write((long)0);
                // a pointer to file table
                binWriter.Write((long)0);
                // file count
                binWriter.Write((long)0);
                stream.Flush();
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
        /// Writes plugin ID, should be called by plugin upon creating a new instance
        /// </summary>
        /// <param name="ID">plugin ID</param>
        public void WritePluginID(long ID)
        {
            lock (streamLock)
            {
                stream.Position = 0;
                // move to format info structure - plugin ID field
                stream.Position = binReader.ReadInt64() + 2 * sizeof(long);
                binWriter.Write(ID);
                stream.Flush();
            }
        }

        /// <summary>
        /// Writes instance name, should be called upon closing the instance
        /// </summary>
        /// <remarks>
        /// Note that repeated calls to this method will not remove previously written content, therefore call it only once.
        /// </remarks>
        /// <param name="name">instance name</param>
        public void WriteInstanceName(string name)
        {
            lock (streamLock)
            {
                stream.Position = 0;
                // move to format info structure - pointer to instance name field
                stream.Position = binReader.ReadInt64() + 3 * sizeof(long);
                binWriter.Write(stream.Length);
                stream.Position = stream.Length;
                formatter.Serialize(stream, name);
            }
        }

        /// <summary>
        /// Writes the channelCount and closes the file
        /// </summary>
        public void Close()
        {
            lock (streamLock)
            {
                if (stream == null) return;
                Debug.WriteLine("FileLog ({0}) closing", filename, 1);
                stream.Position = sizeof(long);
                binWriter.Write(channelCount);
                stream.Position = 0;
                stream.Position = binReader.ReadInt64() + 5 * sizeof(long);
                binWriter.Write(fileCount);
                binWriter.Close();
                binReader = null;
                binWriter = null;
                stream = null;
            }
        }

        /// <summary>
        /// Moves the underlying file to target.
        /// </summary>
        /// <remarks>This method will close the log. Also make sure that there are no active FileReaders for the same file before calling this method.</remarks>
        /// <param name="target">target filename</param>
        public void MoveToFile(string target)
        {
            Close();
            if(File.Exists(target))
            {
                File.Delete(target);
            }
            File.Move(filename, target);
            filename = target;
        }

        /// <summary>
        /// Deletes the underlying file.
        /// </summary>
        /// <inheritdoc cref="FileLog.MoveToFile" select="remarks"/>
        public void DeleteFile()
        {
            Close();
            File.Delete(filename);
            filename = null;
        }

        /// <summary>
        /// Initializes channel info for new channel
        /// </summary>
        /// <returns>hint - pointer to the beginnig of the channel info</returns>
        public long AddChannel()
        {
            long hint = 0;
            lock (streamLock)
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
                stream.Flush();
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
            lock (streamLock)
            {
                Debug.WriteLine("FileLog ({0}) writing channel data (type: {1}, id: {2}, name: {3})", filename, channel.GetType(), channel.ID, channel.Name);
                stream.Position = hint;
                // write the pointer to serialized channel data
                binWriter.Write(stream.Length);
                binWriter.Write((long)eventCount);
                stream.Position = stream.Length;
                formatter.Serialize(stream, channel);
                stream.Flush();
            }
        }

        /// <summary>
        /// Write instance data, usually upon closing the instance. Note: don't call this method repeatedly, because there is no way to delete the old data!
        /// </summary>
        /// <param name="instance"></param>
        public void WriteInstanceData(IInstance instance)
        {
            lock (streamLock)
            {
                Debug.WriteLine("FileLog ({0}) writing instance data (type: {1})", filename, instance.GetType());
                stream.Position = 0;
                stream.Position = binReader.ReadInt64() + sizeof(long);
                // write the pointer to serialized instance data
                binWriter.Write(stream.Length);
                stream.Position = stream.Length;
                formatter.Serialize(stream, instance);
                stream.Flush();
            }
        }

        /// <summary>
        /// Logs event
        /// </summary>
        /// <param name="hint">a pointer to the begging of the channel info, as returned by AddChannel method</param>
        /// <param name="e">event to log</param>
        public void LogEvent(long hint, Event e)
        {
            lock (streamLock)
            {
                Debug.WriteLine("FileLog ({0}) logging event (id: {1}, type: {2})", filename, e.ID, e.Type);
                // move the hint to the beginning of the event table
                hint += 2*sizeof(long);
                logEventHelper(hint, e.ID, e);
                stream.Flush();
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

        /// <summary>
        /// Creates new file for storing larger data
        /// </summary>
        /// <remarks>
        /// Use the file ID to store the information about the file to the event, don't store the hint anywhere it's only valid during one run (in case there will be need to perform offline optimalization of the log file in the future).
        /// Use FileReader.GetFileHint method to obtain a hint when you only know an ID.
        /// ID is 1-based.
        /// </remarks>
        /// <returns>(file ID, file hint)</returns>
        public Tuple<long, long> CreateFile()
        {
            lock(streamLock)
            {
                long id = ++fileCount;
                if(currentFileTable == 0)
                {
                    stream.Position = 0;
                    // move to format info
                    stream.Position = binReader.ReadInt64() + 4*sizeof(long);
                    // create file table and store pointer to it
                    binWriter.Write(stream.Length);
                    currentFileTable = stream.Length;
                    stream.Position = stream.Length;
                    writeNewTable();
                }

                if(++currentFileTableSize == FilesPerBlock)
                {
                    // current file table is full - create a new one
                    stream.Position = currentFileTable;
                    binWriter.Write(stream.Length);
                    stream.Position = stream.Length;
                    currentFileTable = stream.Length;
                    currentFileTableSize = 1;
                    writeNewTable();
                }

                // create new file entry
                stream.Position = currentFileTable + currentFileTableSize * sizeof(long);
                binWriter.Write(stream.Length);
                long fileHint = stream.Length;
                stream.Position = stream.Length;
                // file size
                binWriter.Write((long)0);
                // file block table (1)
                writeNewTable();
                stream.Flush();
                return new Tuple<long,long>(id,fileHint);
            }
        }

        /// <summary>
        /// Append data to file
        /// </summary>
        /// <param name="hint">as returned from FileLog.CreateFile or FileLogReader.GetFileHint</param>
        /// <param name="input">data to append</param>
        /// <remarks>
        /// Note that this function only appends data, eg. be careful not to pass more data into it.
        /// For example if you incrementaly add data to a StreamList don't pass the whole list each time, but only new data.
        /// </remarks>
        public void AppendDataToFile(long hint, IDataStream input)
        {
            long remaining = input.Length;
            if (remaining == 0) return;

            lock(streamLock)
            {
                if(blockBuffer == null)
                {
                    blockBuffer = new byte[BlockSize];
                }
                stream.Position = hint;
                long size = binReader.ReadInt64();
                stream.Position = hint;
                binWriter.Write(size + input.Length);
                // stream position is now the beginning of FBT
                long blockOffset = size % BlockSize;
                long fbt2Offset = (size / (BlockSize)) % (FilesPerBlock + 1);
                long fbtOffset = size / (BlockSize * (FilesPerBlock + 1)) + 1;
                // locate correct FBT
                while(fbtOffset > FilesPerBlock)
                {
                    // move to the next table
                    stream.Position = binReader.ReadInt64();
                    fbtOffset -= FilesPerBlock;
                }
                long currentFBT = stream.Position;
                while(remaining > 0)
                {
                    while (fbtOffset <= FilesPerBlock)
                    {
                        stream.Position = currentFBT + fbtOffset * sizeof(long);
                        long currentFBT2 = binReader.ReadInt64();
                        if (currentFBT2 == 0)
                        {
                            stream.Position -= sizeof(long);
                            binWriter.Write(stream.Length);
                            currentFBT2 = stream.Length;
                            stream.Position = stream.Length;
                            writeNewTable();
                        }
                        while (fbt2Offset <= FilesPerBlock)
                        {
                            int bytesRead = (int)Math.Min(BlockSize - blockOffset, remaining);
                            input.ReadBytesToBuffer(blockBuffer, input.Length - remaining, bytesRead);
                            stream.Position = currentFBT2 + fbt2Offset * sizeof(long);
                            long currentBlock = binReader.ReadInt64();
                            if (currentBlock == 0)
                            {
                                stream.Position -= sizeof(long);
                                binWriter.Write(stream.Length);
                                stream.Position = stream.Length;
                                // blockBuffer is exactly blockSize bytes long
                                // so don't call writeNewTable() to avoid unnecessary writes
                                // extra data written to the block will be ovewritten later or not used at all
                                binWriter.Write(blockBuffer);
                            }
                            else
                            {
                                stream.Position = currentBlock + blockOffset;
                                binWriter.Write(blockBuffer, 0, bytesRead);
                            }
                            blockOffset = 0;
                            remaining -= bytesRead;
                            if (remaining == 0)
                            {
                                return;
                            }
                            // otherwise the current block has been filled completely
                            ++fbt2Offset;
                        }
                        fbt2Offset = 0;
                        fbtOffset++;
                    }
                    fbtOffset = 1;
                    // create new FBT
                    stream.Position = currentFBT;
                    binWriter.Write(stream.Length);
                    currentFBT = stream.Length;
                    stream.Position = stream.Length;
                    writeNewTable();
                }
                stream.Flush();
            }

        }
    }
}