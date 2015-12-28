using Netool.Logging;
using Netool.Network.DataFormats;
using System;
using System.IO;
using Xunit;

namespace Tests.Network.DataFormats
{
    public class LazyLoggedFileTests
    {
        [Fact]
        public void TestSerialization()
        {
            var filename = Path.GetTempFileName();
            var log = new FileLog(filename);
            var innerData = new ByteArray(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var lazy = new LazyLoggedFile(innerData);
            var channel = log.AddChannel();
            log.LogEvent(channel, new Event(1, EventType.RequestReceived, new Netool.Network.DataEventArgs { Data = lazy, State = null }, DateTime.Now));
            var reader = log.CreateReader();
            var e = reader.ReadEvent(channel, 1);
            for (byte i = 0; i < 10; ++i)
            {
                Assert.Equal(i, e.Data.Data.ReadByte(i));
            }
            log.Close();
            reader.Close();
            File.Delete(filename);
        }
    }
}