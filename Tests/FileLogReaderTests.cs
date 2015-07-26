using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netool.Logging;
using Netool.Network;
using Netool.ChannelDrivers;

namespace Tests
{
    [TestClass]
    public class FileLogReaderTests
    {
        private string filename;
        private FileLog log;
        private FileLogReader logReader;

        [TestInitialize]
        public void TestInit()
        {
            filename = Path.GetTempFileName();
            log = new FileLog(filename);
            logReader = log.CreateReader();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            log.Close();
            logReader.Close();
            File.Delete(filename);
        }

        [TestMethod]
        public void TestReadPluginID()
        {
            long pluginID = 17;
            log.WritePluginID(pluginID);
            Assert.AreEqual(pluginID, logReader.ReadPluginID());
        }

        [TestMethod]
        public void TestReadInstanceData()
        {
            var instance = new TestInstance { Serialized = "aaa", NonSerialized = "bbb" };
            log.WriteInstanceData(instance);
            var instance2 = logReader.ReadInstanceData() as TestInstance;
            Assert.IsNotNull(instance2);
            Assert.AreEqual(instance.Serialized, instance2.Serialized);
            Assert.IsTrue(string.IsNullOrEmpty(instance2.NonSerialized));
        }

        [TestMethod]
        public void TestReadChannelsData()
        {
            const int channelCount = 10;
            for(int i = 1; i <= channelCount; ++i)
            {
                var hint = log.AddChannel();
                log.WriteChannelData(hint, 0, new TestChannel { id = i, NonSerialized = "bbb" });
            }
            var channels = logReader.ReadChannelsData(1, channelCount);
            Assert.AreEqual(channelCount, channels.Count);
            for (int i = 1; i <= channelCount; ++i)
            {
                var channel = channels[i - 1] as TestChannel;
                Assert.IsNotNull(channel);
                Assert.AreEqual(i, channel.id);
                Assert.IsTrue(string.IsNullOrEmpty(channel.NonSerialized));
            }
            channels = logReader.ReadChannelsData(3, 1);
            Assert.AreEqual(1, channels.Count);
            var channel2 = channels[0] as TestChannel;
            Assert.IsNotNull(channel2);
            Assert.AreEqual(3, channel2.id);
            channels = logReader.ReadChannelsData(3, 0);
            Assert.AreEqual(0, channels.Count);
        }

        [TestMethod]
        public void TestReadChannelData()
        {
            var channel = new TestChannel { id = 55, NonSerialized = "bbb" };
            var hint = log.AddChannel();
            log.WriteChannelData(hint, 0, channel);
            var channel2 = logReader.ReadChannelData(1) as TestChannel;
            Assert.IsNotNull(channel2);
            Assert.AreEqual(55, channel2.ID);
            Assert.IsTrue(string.IsNullOrEmpty(channel2.NonSerialized));
        }

        [TestMethod]
        public void TestGetChannelInfoHintByID()
        {
            var hint1 = log.AddChannel();
            var hint2 = log.AddChannel();
            Assert.AreEqual(hint1, logReader.GetChannelInfoHintByID(1));
            Assert.AreEqual(hint2, logReader.GetChannelInfoHintByID(2));
        }

        [TestMethod]
        public void TestGetEventCount()
        {
            var hint = log.AddChannel();
            log.LogEvent(hint, new Event(1, EventType.ChannelCreated, null, DateTime.Now));
            log.LogEvent(hint, new Event(2, EventType.RequestReceived, null, DateTime.Now));
            log.WriteChannelData(hint, 2, new TestChannel());
            var hint2 = log.AddChannel();
            log.LogEvent(hint2, new Event(1, EventType.ChannelCreated, null, DateTime.Now));
            log.WriteChannelData(hint2, 1, new TestChannel());
            Assert.AreEqual(1, logReader.GetEventCount(hint2));
            Assert.AreEqual(2, logReader.GetEventCount(hint));
        }

        [TestMethod]
        public void TestReadEvent()
        {
            var hint = log.AddChannel();
            DateTime date1 = DateTime.Parse("01/01/2005 21:21:21");
            DateTime date2 = DateTime.Parse("01/01/2006 21:21:21");
            log.LogEvent(hint, new Event(1, EventType.ChannelCreated, null, date1));
            log.LogEvent(hint, new Event(2, EventType.ChannelClosed, null, date2));
            var e1 = logReader.ReadEvent(hint, 1);
            Assert.IsNotNull(e1);
            Assert.AreEqual(EventType.ChannelCreated, e1.Type);
            Assert.AreEqual(1, e1.ID);
            Assert.AreEqual(date1, e1.Time);
            var e2 = logReader.ReadEvent(hint, 2);
            Assert.IsNotNull(e2);
            Assert.AreEqual(EventType.ChannelClosed, e2.Type);
            Assert.AreEqual(2, e2.ID);
            Assert.AreEqual(date2, e2.Time);
        }

        [TestMethod]
        public void TestReadEvents()
        {
            const int eventCount = FileLog.EventsPerBlock + 1;
            var hint = log.AddChannel();

            for(int i = 1; i <= eventCount; ++i)
            {
                log.LogEvent(hint, new Event(i, EventType.ChannelCreated, null, DateTime.Now));
            }
            var events = logReader.ReadEvents(hint, 1, 0);
            Assert.AreEqual(0, events.Count);
            events = logReader.ReadEvents(hint, 1, eventCount);
            Assert.AreEqual(eventCount, events.Count);
            Assert.AreEqual(1, events[0].ID);
            Assert.AreEqual(2, events[1].ID);
            Assert.AreEqual(eventCount, events[eventCount -1].ID);
            events = logReader.ReadEvents(hint, 1, 1);
            Assert.AreEqual(1, events.Count);
        }
    }
}
