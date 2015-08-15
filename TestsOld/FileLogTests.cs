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
    public class FileLogTests
    {
        private string filename;
        private FileLog log;

        [TestInitialize]
        public void TestInit()
        {
            filename = Path.GetTempFileName();
            log = new FileLog(filename);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(filename);
        }

        [TestMethod]
        public void TestFormatVersion()
        {
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            stream.Position = 0;
            var fInfo = binReader.ReadInt64();
            Assert.IsTrue(fInfo > 0);
            // log contains pluginID field
            Assert.IsTrue(stream.Length >= fInfo);
            stream.Position = fInfo;
            Assert.AreEqual(FileLog.FormatVersion, binReader.ReadInt64());
            binReader.Close();
        }

        [TestMethod]
        public void TestWritePluginID()
        {
            long pluginID = 17;
            log.WritePluginID(pluginID);
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            stream.Position = 0;
            var fInfo = binReader.ReadInt64();
            Assert.IsTrue(fInfo > 0);
            // log contains pluginID field
            Assert.IsTrue(stream.Length >= fInfo + 2 * sizeof(long));
            stream.Position = fInfo + 2 * sizeof(long);
            Assert.AreEqual(pluginID, binReader.ReadInt64());
            binReader.Close();
        }

        [TestMethod]
        public void TestAddChannel0()
        {
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            // log file contains ChannelCount field
            Assert.IsTrue(stream.Length >= 2 * sizeof(long));
            stream.Position = sizeof(long);
            // 0 channels
            Assert.AreEqual(0, binReader.ReadInt64());
            binReader.Close();
        }

        [TestMethod]
        public void TestAddChannel()
        {
            log.AddChannel();
            log.AddChannel();
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            stream.Position = sizeof(long);
            // 2 channels
            Assert.AreEqual(2, binReader.ReadInt64());
            // skip next channel table pointer
            stream.Position += sizeof(long);
            long channelPos = binReader.ReadInt64();
            // check the first channel
            Assert.IsTrue(channelPos > 0);
            Assert.IsTrue(stream.Length > channelPos + sizeof(long));
            // move to event count
            stream.Position = channelPos + sizeof(long);
            Assert.AreEqual(0, binReader.ReadInt64());
            // move to the second channel pointer
            stream.Position = 4 * sizeof(long);
            channelPos = binReader.ReadInt64();
            // check the second channel
            Assert.IsTrue(channelPos > 0);
            Assert.IsTrue(stream.Length > channelPos + sizeof(long));
            // move to event count
            stream.Position = channelPos + sizeof(long);
            Assert.AreEqual(0, binReader.ReadInt64());
            binReader.Close();
        }

        [TestMethod]
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
                Assert.AreEqual(FileLog.ChannelsPerBlock + 1, binReader.ReadInt64());
                // the last channel will be first in the second table
                stream.Position = binReader.ReadInt64() + sizeof(long);
                Assert.AreEqual(hint, binReader.ReadInt64());
                binReader.Close();
            }

        }

        [TestMethod]
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
                Assert.IsTrue(instancePos > 0);
                Assert.IsTrue(stream.Length > instancePos);
                stream.Position = instancePos;
                object res = formatter.Deserialize(stream);
                Assert.IsInstanceOfType(res, typeof(TestInstance));
                var ins = res as TestInstance;
                Assert.IsTrue(String.IsNullOrEmpty(ins.NonSerialized));
                Assert.AreEqual("aaa", ins.Serialized);
                binReader.Close();
            }
        }

        [TestMethod]
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
                Assert.AreEqual(hint, binReader.ReadInt64());
                Assert.AreEqual(hint2, binReader.ReadInt64());
                stream.Position = hint + sizeof(long);
                // check that eventcount is properly written
                Assert.AreEqual(5, binReader.ReadInt64());
                // check that object can be properly deserialized
                stream.Position = hint;
                stream.Position = binReader.ReadInt64();
                object res = formatter.Deserialize(stream);
                Assert.IsInstanceOfType(res, typeof(TestChannel));
                var c = res as TestChannel;
                Assert.IsTrue(String.IsNullOrEmpty(c.NonSerialized));
                Assert.AreEqual(1, c.id);

                // second channel
                stream.Position = hint2;
                stream.Position = binReader.ReadInt64();
                res = formatter.Deserialize(stream);
                Assert.IsInstanceOfType(res, typeof(TestChannel));
                c = res as TestChannel;
                Assert.IsTrue(String.IsNullOrEmpty(c.NonSerialized));
                Assert.AreEqual(2, c.id);
                binReader.Close();
            }
        }

        [TestMethod]
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
                Assert.IsInstanceOfType(res, typeof(Event));
                var e = res as Event;
                Assert.AreEqual(1, e.ID);
                Assert.AreEqual(EventType.ChannelCreated, e.Type);
                Assert.AreEqual(date1, e.Time);

                stream.Position = 3 * sizeof(long);
                // move to channel info structure
                stream.Position = binReader.ReadInt64();
                // second event
                stream.Position += 4 * sizeof(long);
                stream.Position = binReader.ReadInt64();

                res = formatter.Deserialize(stream);
                Assert.IsInstanceOfType(res, typeof(Event));
                e = res as Event;
                Assert.AreEqual(2, e.ID);
                Assert.AreEqual(EventType.ChannelClosed, e.Type);
                Assert.AreEqual(date2, e.Time);
                binReader.Close();
            }
        }

        [TestMethod]
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
                Assert.IsInstanceOfType(res, typeof(Event));
                var e = res as Event;
                Assert.AreEqual(FileLog.EventsPerBlock + 1, e.ID);
                binReader.Close();
            }
        }

    }
}
