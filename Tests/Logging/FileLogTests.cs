using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using Netool.Logging;
using Netool.Network;
using Netool.Network.DataFormats;
using Netool.ChannelDrivers;

namespace Tests.Logging
{
    public static class FileLogTestsHelper
    {

        /// <summary>
        /// Creates a mock of big file (Max size per FBT - 1B) inside a FileLog's file specified by the hint. Use only with empty file.
        /// </summary>
        /// <param name="hint"></param>
        /// <param name="stream"></param>
        /// <returns>new file size</returns>
        public static long CreateBigLogFile(long hint, FileStream stream)
        {
            long newFileSize = FileLog.FilesPerBlock * (FileLog.FilesPerBlock + 1) * FileLog.BlockSize - 1;
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                using (BinaryWriter binWriter = new BinaryWriter(stream))
                {
                    // a pointer to file
                    stream.Position = hint;
                    // file size - make it max length per FBT - 1B
                    // to test writing when new data block, new FBT2 and new FBT are necessary
                    binWriter.Write(newFileSize);
                    // now create the necessary strucure
                    // move to the last entry in FBT
                    stream.Position += FileLog.FilesPerBlock * sizeof(long);
                    binWriter.Write(stream.Length);
                    stream.Position = stream.Length;
                    for (int i = 0; i < FileLog.BlockSize; ++i)
                    {
                        binWriter.Write((byte)0);
                    }
                    // set the last entry in FBT2
                    stream.Position -= sizeof(long);
                    binWriter.Write(stream.Length);
                    stream.Position = stream.Length;
                    // prepare data block
                    for (int i = 0; i < FileLog.BlockSize; ++i)
                    {
                        binWriter.Write((byte)(i % 128));
                    }
                    binWriter.Close();
                }
            }
            return newFileSize;
        }
    }
    public class FileLogTests : IDisposable
    {
        private string filename;
        private FileLog log;

        public FileLogTests()
        {
            filename = Path.GetTempFileName();
            log = new FileLog(filename);
        }

        void IDisposable.Dispose()
        {
            File.Delete(filename);
        }

        [Fact]
        public void TestFormatVersion()
        {
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            stream.Position = 0;
            var fInfo = binReader.ReadInt64();
            Assert.True(fInfo > 0);
            // log contains pluginID field
            Assert.True(stream.Length >= fInfo);
            stream.Position = fInfo;
            Assert.Equal(FileLog.FormatVersion, binReader.ReadInt64());
            binReader.Close();
        }

        [Fact]
        public void TestWritePluginID()
        {
            long pluginID = 17;
            log.WritePluginID(pluginID);
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            stream.Position = 0;
            var fInfo = binReader.ReadInt64();
            Assert.True(fInfo > 0);
            // log contains pluginID field
            Assert.True(stream.Length >= fInfo + 2 * sizeof(long));
            stream.Position = fInfo + 2 * sizeof(long);
            Assert.Equal(pluginID, binReader.ReadInt64());
            binReader.Close();
        }

        [Fact]
        public void TestAddChannel0()
        {
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            // log file contains ChannelCount field
            Assert.True(stream.Length >= 2 * sizeof(long));
            stream.Position = sizeof(long);
            // 0 channels
            Assert.Equal(0, binReader.ReadInt64());
            binReader.Close();
        }

        [Fact]
        public void TestAddChannel()
        {
            log.AddChannel();
            log.AddChannel();
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            stream.Position = sizeof(long);
            // 2 channels
            Assert.Equal(2, binReader.ReadInt64());
            // skip next channel table pointer
            stream.Position += sizeof(long);
            long channelPos = binReader.ReadInt64();
            // check the first channel
            Assert.True(channelPos > 0);
            Assert.True(stream.Length > channelPos + sizeof(long));
            // move to event count
            stream.Position = channelPos + sizeof(long);
            Assert.Equal(0, binReader.ReadInt64());
            // move to the second channel pointer
            stream.Position = 4 * sizeof(long);
            channelPos = binReader.ReadInt64();
            // check the second channel
            Assert.True(channelPos > 0);
            Assert.True(stream.Length > channelPos + sizeof(long));
            // move to event count
            stream.Position = channelPos + sizeof(long);
            Assert.Equal(0, binReader.ReadInt64());
            binReader.Close();
        }

        [Fact]
        public void TestManyAddChannel()
        {
            long hint = 0;
            for (int i = 0; i <= FileLog.ChannelsPerBlock; ++i )
            {
                hint = log.AddChannel();
            }
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using(BinaryReader binReader = new BinaryReader(stream))
            {
                stream.Position = sizeof(long);
                // channel count
                Assert.Equal(FileLog.ChannelsPerBlock + 1, binReader.ReadInt64());
                // the last channel will be first in the second table
                stream.Position = binReader.ReadInt64() + sizeof(long);
                Assert.Equal(hint, binReader.ReadInt64());
                binReader.Close();
            }

        }

        [Fact]
        public void TestWriteInstanceData()
        {
            log.WriteInstanceData(new TestInstance { Serialized = "aaa", NonSerialized = "bbb" });
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using(BinaryReader binReader = new BinaryReader(stream))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                stream.Position = 0;
                // move to format info structure - instance data field
                stream.Position = binReader.ReadInt64() + sizeof(long);
                var instancePos = binReader.ReadInt64();
                Assert.True(instancePos > 0);
                Assert.True(stream.Length > instancePos);
                stream.Position = instancePos;
                object res = formatter.Deserialize(stream);
                Assert.IsType(typeof(TestInstance), res);
                var ins = res as TestInstance;
                Assert.True(String.IsNullOrEmpty(ins.NonSerialized));
                Assert.Equal("aaa", ins.Serialized);
                binReader.Close();
            }
        }

        [Fact]
        public void TestWriteChannelData()
        {
            var hint = log.AddChannel();
            var hint2 = log.AddChannel();
            log.WriteChannelData(hint, 5, new TestChannel { id = 1, NonSerialized = "bbb" });
            log.WriteChannelData(hint2, 7, new TestChannel { id = 2, NonSerialized = "ccc" });
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using(BinaryReader binReader = new BinaryReader(stream))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                stream.Position = 3 * sizeof(long);
                // check that pointers are properly stored in the table
                Assert.Equal(hint, binReader.ReadInt64());
                Assert.Equal(hint2, binReader.ReadInt64());
                stream.Position = hint + sizeof(long);
                // check that eventcount is properly written
                Assert.Equal(5, binReader.ReadInt64());
                // check that object can be properly deserialized
                stream.Position = hint;
                stream.Position = binReader.ReadInt64();
                object res = formatter.Deserialize(stream);
                Assert.IsType(typeof(TestChannel), res);
                var c = res as TestChannel;
                Assert.True(String.IsNullOrEmpty(c.NonSerialized));
                Assert.Equal(1, c.id);

                // second channel
                stream.Position = hint2;
                stream.Position = binReader.ReadInt64();
                res = formatter.Deserialize(stream);
                Assert.IsType(typeof(TestChannel), res);
                c = res as TestChannel;
                Assert.True(String.IsNullOrEmpty(c.NonSerialized));
                Assert.Equal(2, c.id);
                binReader.Close();
            }
        }

        [Fact]
        public void TestLogEvent()
        {
            var hint = log.AddChannel();
            DateTime date1 = DateTime.Parse("01/01/2005 21:21:21");
            DateTime date2 = DateTime.Parse("01/01/2006 21:21:21");
            log.LogEvent(hint, new Event(1, EventType.ChannelCreated, null, date1));
            log.LogEvent(hint, new Event(2, EventType.ChannelClosed, null, date2));
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using(BinaryReader binReader = new BinaryReader(stream))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                stream.Position = 3 * sizeof(long);
                // move to channel info structure
                stream.Position = binReader.ReadInt64();
                // first event
                stream.Position += 3 * sizeof(long);
                stream.Position = binReader.ReadInt64();
                object res = formatter.Deserialize(stream);
                Assert.IsType(typeof(Event), res);
                var e = res as Event;
                Assert.Equal(1, e.ID);
                Assert.Equal(EventType.ChannelCreated, e.Type);
                Assert.Equal(date1, e.Time);

                stream.Position = 3 * sizeof(long);
                // move to channel info structure
                stream.Position = binReader.ReadInt64();
                // second event
                stream.Position += 4 * sizeof(long);
                stream.Position = binReader.ReadInt64();

                res = formatter.Deserialize(stream);
                Assert.IsType(typeof(Event), res);
                e = res as Event;
                Assert.Equal(2, e.ID);
                Assert.Equal(EventType.ChannelClosed, e.Type);
                Assert.Equal(date2, e.Time);
                binReader.Close();
            }
        }

        [Fact]
        public void TestManyLogEvent()
        {
            var hint = log.AddChannel();
            for (int i = 0; i < FileLog.EventsPerBlock; i++ )
            {
                log.LogEvent(hint, new Event(i+1, EventType.ChannelCreated, null, DateTime.Now));
            }
            log.LogEvent(hint, new Event(FileLog.EventsPerBlock + 1, EventType.ChannelCreated, null, DateTime.Now));
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                // jump to channel info
                stream.Position = hint;
                // skip channel data pointer and eventCount (is only written with WriteChannelData method)
                stream.Position += 2*sizeof(long);
                // move to the next table, first event
                stream.Position = binReader.ReadInt64() + sizeof(long);
                // move to the event data
                stream.Position = binReader.ReadInt64();
                object res = formatter.Deserialize(stream);
                Assert.IsType(typeof(Event), res);
                var e = res as Event;
                Assert.Equal(FileLog.EventsPerBlock + 1, e.ID);
                binReader.Close();
            }
        }

        [Fact]
        public void TestWriteInstanceName()
        {
            var instanceName = "instance name";
            log.WriteInstanceName(instanceName);
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                // a pointer to instance info
                stream.Position = 0;
                // a pointer to instance name pointer
                stream.Position = binReader.ReadInt64() + 3 * sizeof(long);
                // instance name
                var pos = binReader.ReadInt64();
                Assert.NotEqual(0, pos);
                stream.Position = pos;
                object res = formatter.Deserialize(stream);
                Assert.IsType(typeof(string), res);
                var name = res as string;
                Assert.Equal(instanceName, name);
                binReader.Close();
            }
        }

        [Fact]
        public void TestCreateFile_None()
        {
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                // a pointer to format info
                stream.Position = 0;
                // a pointer to file table
                stream.Position = binReader.ReadInt64() + 4 * sizeof(long);
                // don't initialize file table unless some file was actually created
                Assert.Equal(0, binReader.ReadInt64());
                // file count == 0
                Assert.Equal(0, binReader.ReadInt64());
                binReader.Close();
            }
        }

        [Fact]
        public void TestCreateFile_One()
        {
            log.CreateFile();
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                // a pointer to format info
                stream.Position = 0;
                // a pointer to file table
                stream.Position = binReader.ReadInt64() + 4 * sizeof(long);
                var fileTable = binReader.ReadInt64();
                Assert.InRange(fileTable, 1, stream.Length - 1);
                // file count
                Assert.Equal(1, binReader.ReadInt64());
                stream.Position = fileTable;
                // check that there is no other file table
                Assert.Equal(0, binReader.ReadInt64());
                long filePtr = binReader.ReadInt64();
                Assert.InRange(filePtr, 1, stream.Length - 1);
                stream.Position = filePtr;
                // file size
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - next pointer
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - first file entry
                Assert.Equal(0, binReader.ReadInt64());
                binReader.Close();
            }
        }

        [Fact]
        public void TestCreateFile_Many()
        {
            for (int i = 0; i < FileLog.FilesPerBlock + 1; ++i)
            {
                log.CreateFile();
            }
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                // a pointer to format info
                stream.Position = 0;
                // a pointer to file table
                stream.Position = binReader.ReadInt64() + 4 * sizeof(long);
                var fileTable = binReader.ReadInt64();
                Assert.InRange(fileTable, 1, stream.Length - 1);
                // file count
                Assert.Equal(FileLog.FilesPerBlock + 1, binReader.ReadInt64());
                stream.Position = fileTable;
                // check that there is no other file table
                long nextFileTable = binReader.ReadInt64();
                Assert.InRange(nextFileTable, 1, stream.Length);
                stream.Position = nextFileTable;
                // no third file table
                Assert.Equal(0, binReader.ReadInt64());
                long filePtr = binReader.ReadInt64();
                Assert.InRange(filePtr, 1, stream.Length - 1);
                stream.Position = filePtr;
                // file size
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - next pointer
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - first file entry
                Assert.Equal(0, binReader.ReadInt64());
                binReader.Close();
            }
        }

        [Fact]
        public void TestCreateAndAppendFile_OneBlock()
        {
            var file1 = log.CreateFile();
            log.AppendDataToFile(file1.Item2, new ByteArray(new byte[] { 1, 2, 3, 4 }));
            log.AppendDataToFile(file1.Item2, new ByteArray(new byte[] { 5, 6, 7, 8 }));
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                // a pointer to format info
                stream.Position = 0;
                // a pointer to file table
                stream.Position = binReader.ReadInt64() + 4 * sizeof(long);
                var fileTable = binReader.ReadInt64();
                Assert.InRange(fileTable, 1, stream.Length - 1);
                // file count
                Assert.Equal(1, binReader.ReadInt64());
                // skip file table next pointer
                stream.Position = fileTable + sizeof(long);
                long filePtr = binReader.ReadInt64();
                Assert.InRange(filePtr, 1, stream.Length - 1);
                stream.Position = filePtr;
                // file size
                Assert.Equal(8, binReader.ReadInt64());
                // FBT - next pointer
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - first FBT2 entry
                long fbt2Ptr = binReader.ReadInt64();
                Assert.InRange(fbt2Ptr, 1, stream.Length - 1);
                stream.Position = fbt2Ptr;
                long dataPtr = binReader.ReadInt64();
                Assert.InRange(dataPtr, 1, stream.Length - 1);
                stream.Position = dataPtr;
                for (int i = 1; i <= 8; i++ )
                {
                    Assert.Equal(i, binReader.ReadByte());
                }
                binReader.Close();
            }
        }

        [Fact]
        public void TestCreateAndAppendFile_Big()
        {
            var file1 = log.CreateFile();
            log.Close();
            // a little hack to avoid writing gigabyte of data
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            var newFileSize = FileLogTestsHelper.CreateBigLogFile(file1.Item2, stream);
            log = new FileLog(filename, FileMode.Open);
            // Don't use hint from previously open log file
            log.AppendDataToFile(file1.Item2, new ByteArray(new byte[] { 233, 234, 235, 236 }));
            log.AppendDataToFile(file1.Item2, new ByteArray(new byte[] { 237, 238, 239 }));
            log.Close();
            stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                stream.Position = 0;
                // move to file table pointer
                stream.Position = binReader.ReadInt64() + 4*sizeof(long);
                // move to pointer to file 1
                stream.Position = binReader.ReadInt64() + sizeof(long);
                // move to file 1 - file size
                stream.Position = binReader.ReadInt64();
                Assert.Equal(newFileSize + 7, binReader.ReadInt64());
                long nextFBT = binReader.ReadInt64();
                Assert.InRange(nextFBT, 1, stream.Length);
                // move to last FBT entry (next pointer was read already)
                stream.Position += (FileLog.FilesPerBlock - 1) * sizeof(long);
                // move to last FBT2 entry
                stream.Position = binReader.ReadInt64() + FileLog.FilesPerBlock * sizeof(long);
                // move to data block
                stream.Position = binReader.ReadInt64();
                for(int i = 0; i <  FileLog.BlockSize - 1; ++i)
                {
                    // check that previous data were not altered
                    Assert.Equal(i % 128, binReader.ReadByte());
                }
                Assert.Equal(233, binReader.ReadByte());
                // move to second FBT
                stream.Position = nextFBT;
                // no third FBT
                Assert.Equal(0, binReader.ReadInt64());
                long fbt2Ptr = binReader.ReadInt64();
                Assert.InRange(fbt2Ptr, 1, stream.Length);
                // no other entry in the second FBT
                Assert.Equal(0, binReader.ReadInt64());
                stream.Position = fbt2Ptr;
                long dataPtr = binReader.ReadInt64();
                Assert.InRange(dataPtr, 1, stream.Length);
                stream.Position = dataPtr;
                for(int i = 0; i < 6; ++i)
                {
                    var b = binReader.ReadByte();
                    Assert.Equal(234 + i, b);
                }
            }
        }

        [Fact]
        public void TestCreateAndAppendFile_Empty()
        {
            var file1 = log.CreateFile();
            log.AppendDataToFile(file1.Item2, new EmptyData());
            log.AppendDataToFile(file1.Item2, new EmptyData());
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                // a pointer to format info
                stream.Position = 0;
                // a pointer to file table
                stream.Position = binReader.ReadInt64() + 4 * sizeof(long);
                var fileTable = binReader.ReadInt64();
                Assert.InRange(fileTable, 1, stream.Length - 1);
                // file count
                Assert.Equal(1, binReader.ReadInt64());
                // skip file table next pointer
                stream.Position = fileTable + sizeof(long);
                long filePtr = binReader.ReadInt64();
                Assert.InRange(filePtr, 1, stream.Length - 1);
                stream.Position = filePtr;
                // file size
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - next pointer
                Assert.Equal(0, binReader.ReadInt64());
                // FBT - first FBT2 entry
                Assert.Equal(0, binReader.ReadInt64());
            }
        }
    }
}
