using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netool.Logging;
using Netool.Network;
using Netool.ChannelDrivers;

namespace Tests
{
    [Serializable]
    internal class TestInstance : IInstance
    {
        public string Serialized;
        [NonSerialized]
        public string NonSerialized;
        public bool IsStarted { get { return true; } }
        public void Stop()
        { }
    }

    [Serializable]
    internal class TestChannel : IChannel
    {
        public int id;
        public int ID { get { return id; } }
        public string name = "aaa";
        public string Name { get { return name; } }
        public IChannelDriver Driver { get { return null; } set {} }
        public event ChannelReadyHandler ChannelReady;
        public event ChannelClosedHandler ChannelClosed;
        public void Close() { }
        [NonSerialized]
        public string NonSerialized;
    }

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
        public void TestAddChannel0()
        {
            log.Close();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(stream);
            Assert.AreEqual(2 * sizeof(long) + FileLog.BlockSize, stream.Length);
            stream.Position = 0;
            // no pointer to instance info
            Assert.AreEqual(0, binReader.ReadInt64());
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
            // longs - pointer to instance data, channel count, 2* (pointer to channel data, eventCount)
            Assert.AreEqual(6 * sizeof(long) + 3*FileLog.BlockSize, stream.Length);
            stream.Position = 0;
            // no pointer to instance info
            Assert.AreEqual(0, binReader.ReadInt64());
            // 2 channels
            Assert.AreEqual(2, binReader.ReadInt64());
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
                stream.Position = 0;
                // no pointer to instance info
                Assert.AreEqual(0, binReader.ReadInt64());
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
                stream.Position = binReader.ReadInt64();
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
