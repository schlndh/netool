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
    public class FileLogReaderTests : IDisposable
    {
        private string filename;
        private FileLog log;
        private FileLogReader logReader;

        public FileLogReaderTests()
        {
            filename = Path.GetTempFileName();
            log = new FileLog(filename);
            logReader = log.CreateReader();
        }

        void IDisposable.Dispose()
        {
            log.Close();
            logReader.Close();
            File.Delete(filename);
        }

        [Fact]
        public void TestReadPluginID()
        {
            long pluginID = 17;
            log.WritePluginID(pluginID);
            Assert.Equal(pluginID, logReader.ReadPluginID());
        }

        [Fact]
        public void TestReadInstanceData()
        {
            var instance = new TestInstance { Serialized = "aaa", NonSerialized = "bbb" };
            log.WriteInstanceData(instance);
            var instance2 = logReader.ReadInstanceData() as TestInstance;
            Assert.NotNull(instance2);
            Assert.Equal(instance.Serialized, instance2.Serialized);
            Assert.True(string.IsNullOrEmpty(instance2.NonSerialized));
        }

        [Fact]
        public void TestReadChannelsData()
        {
            const int channelCount = 10;
            for(int i = 1; i <= channelCount; ++i)
            {
                var hint = log.AddChannel();
                log.WriteChannelData(hint, 0, new TestChannel { id = i, NonSerialized = "bbb" });
            }
            var channels = logReader.ReadChannelsData(1, channelCount);
            Assert.Equal(channelCount, channels.Count);
            for (int i = 1; i <= channelCount; ++i)
            {
                var channel = channels[i - 1] as TestChannel;
                Assert.NotNull(channel);
                Assert.Equal(i, channel.id);
                Assert.True(string.IsNullOrEmpty(channel.NonSerialized));
            }
            channels = logReader.ReadChannelsData(3, 1);
            Assert.Equal(1, channels.Count);
            var channel2 = channels[0] as TestChannel;
            Assert.NotNull(channel2);
            Assert.Equal(3, channel2.id);
            channels = logReader.ReadChannelsData(3, 0);
            Assert.Equal(0, channels.Count);
        }

        [Fact]
        public void TestReadChannelData()
        {
            var channel = new TestChannel { id = 55, NonSerialized = "bbb" };
            var hint = log.AddChannel();
            log.WriteChannelData(hint, 0, channel);
            var channel2 = logReader.ReadChannelData(1) as TestChannel;
            Assert.NotNull(channel2);
            Assert.Equal(55, channel2.ID);
            Assert.True(string.IsNullOrEmpty(channel2.NonSerialized));
        }

        [Fact]
        public void TestGetChannelInfoHintByID()
        {
            var hint1 = log.AddChannel();
            var hint2 = log.AddChannel();
            Assert.Equal(hint1, logReader.GetChannelInfoHintByID(1));
            Assert.Equal(hint2, logReader.GetChannelInfoHintByID(2));
        }

        [Fact]
        public void TestGetEventCount()
        {
            var hint = log.AddChannel();
            log.LogEvent(hint, new Event(1, EventType.ChannelCreated, null, DateTime.Now));
            log.LogEvent(hint, new Event(2, EventType.RequestReceived, null, DateTime.Now));
            log.WriteChannelData(hint, 2, new TestChannel());
            var hint2 = log.AddChannel();
            log.LogEvent(hint2, new Event(1, EventType.ChannelCreated, null, DateTime.Now));
            log.WriteChannelData(hint2, 1, new TestChannel());
            Assert.Equal(1, logReader.GetEventCount(hint2));
            Assert.Equal(2, logReader.GetEventCount(hint));
        }

        [Fact]
        public void TestReadEvent()
        {
            var hint = log.AddChannel();
            DateTime date1 = DateTime.Parse("01/01/2005 21:21:21");
            DateTime date2 = DateTime.Parse("01/01/2006 21:21:21");
            log.LogEvent(hint, new Event(1, EventType.ChannelCreated, null, date1));
            log.LogEvent(hint, new Event(2, EventType.ChannelClosed, null, date2));
            var e1 = logReader.ReadEvent(hint, 1);
            Assert.NotNull(e1);
            Assert.Equal(EventType.ChannelCreated, e1.Type);
            Assert.Equal(1, e1.ID);
            Assert.Equal(date1, e1.Time);
            var e2 = logReader.ReadEvent(hint, 2);
            Assert.NotNull(e2);
            Assert.Equal(EventType.ChannelClosed, e2.Type);
            Assert.Equal(2, e2.ID);
            Assert.Equal(date2, e2.Time);
        }

        [Fact]
        public void TestReadEvents()
        {
            const int eventCount = FileLog.EventsPerBlock + 1;
            var hint = log.AddChannel();

            for(int i = 1; i <= eventCount; ++i)
            {
                log.LogEvent(hint, new Event(i, EventType.ChannelCreated, null, DateTime.Now));
            }
            var events = logReader.ReadEvents(hint, 1, 0);
            Assert.Equal(0, events.Count);
            events = logReader.ReadEvents(hint, 1, eventCount);
            Assert.Equal(eventCount, events.Count);
            Assert.Equal(1, events[0].ID);
            Assert.Equal(2, events[1].ID);
            Assert.Equal(eventCount, events[eventCount -1].ID);
            events = logReader.ReadEvents(hint, 1, 1);
            Assert.Equal(1, events.Count);
        }

        [Fact]
        public void TestReadInstanceName()
        {
            // no instance name has been written yet
            Assert.Null(logReader.ReadInstanceName());
            var instanceName = "instance name";
            log.WriteInstanceName(instanceName);
            Assert.Equal(instanceName, logReader.ReadInstanceName());
        }

        [Fact]
        public void TestGetFileHint_Ok()
        {
            var file = log.CreateFile();
            Assert.Equal(file.Hint, logReader.GetFileHint(file.ID));
        }

        [Fact]
        public void TestGetFileHint_BadId()
        {
            Assert.Equal(0, logReader.GetFileHint(1));
        }

        [Fact]
        public void TestGetFileLength()
        {
            var hint = log.CreateFile().Hint;
            Assert.Equal(0, logReader.GetFileLength(hint));
            log.AppendDataToFile(hint, new ByteArray(new byte[] { 1, 2, 3, 4 }));
            Assert.Equal(4, logReader.GetFileLength(hint));
            log.AppendDataToFile(hint, new ByteArray(new byte[] { 1, 2, 3, 4 }));
            Assert.Equal(8, logReader.GetFileLength(hint));
        }

        [Fact]
        public void TestReadFileDataToBuffer_Normal()
        {
            var hint = log.CreateFile().Hint;
            byte[] buffer = new byte[128];
            buffer[0] = 15;
            Assert.Throws(typeof(IndexOutOfRangeException), () => logReader.ReadFileDataToBuffer(hint, buffer, 0, 1, 0));
            Assert.Throws(typeof(IndexOutOfRangeException), () => logReader.ReadFileDataToBuffer(hint, buffer, 1, 0, 0));
            Assert.Throws(typeof(IndexOutOfRangeException), () => logReader.ReadFileDataToBuffer(hint, buffer, 1, 1, 0));
            logReader.ReadFileDataToBuffer(hint, buffer, 0, 0, 0);
            Assert.Equal(15, buffer[0]);
            log.AppendDataToFile(hint, new ByteArray(new byte[] { 1, 2, 3, 4 }));
            Assert.Throws(typeof(IndexOutOfRangeException), () => logReader.ReadFileDataToBuffer(hint, buffer, 4, 1, 0));
            Assert.Equal(15, buffer[0]);
            logReader.ReadFileDataToBuffer(hint, buffer, 1, 2, 1);
            Assert.Equal(15, buffer[0]);
            Assert.Equal(2, buffer[1]);
            Assert.Equal(3, buffer[2]);
        }

        [Fact]
        public void TestReadFileDataToBuffer_ReadAcrossFBT2()
        {
            var hint = log.CreateFile().Hint;
            byte[] data = new byte[256];
            for(int i = 0; i < 256; ++i)
            {
                data[i] = (byte)i;
            }
            var dataStream = new ByteArray(data);

            for (int i = 0; i < (FileLog.FilesPerBlock+1)*FileLog.BlockSize; i += (int)dataStream.Length )
            {
                log.AppendDataToFile(hint, dataStream);
            }
            Array.Reverse(data);
            log.AppendDataToFile(hint, new ByteArray(data));
            var buffer = new byte[FileLog.BlockSize];
            logReader.ReadFileDataToBuffer(hint, buffer, logReader.GetFileLength(hint) - buffer.Length, buffer.Length, 0);
            for (int i = 256; i > 0; --i)
            {
                Assert.Equal(i - 1, buffer[buffer.Length - i]);
            }
        }

        [Fact]
        public void TestReadFileDataToBuffer_Big()
        {
            var hint = log.CreateFile().Hint;
            logReader.Close();
            log.Close();
            var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var newFileSize = FileLogTestsHelper.CreateBigLogFile(hint, stream);
            stream.Close();
            log = new FileLog(filename, FileMode.Open);
            log.AppendDataToFile(hint, new ByteArray(new byte[] { 1, 2, 3, 4 }));
            byte[] buffer = new byte[128];
            buffer[0] = 15;
            logReader = log.CreateReader();
            logReader.ReadFileDataToBuffer(hint, buffer, newFileSize, 4, 1);
            Assert.Equal(15, buffer[0]);
            Assert.Equal(1, buffer[1]);
            Assert.Equal(2, buffer[2]);
        }

        [Fact]
        public void TestDispose()
        {
            var reader = log.CreateReader();
            reader.Dispose();
            Assert.True(reader.IsClosed);
        }
    }
}
