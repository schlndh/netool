using Netool.Logging;
using Netool.Network.DataFormats;
using System;
using System.IO;
using Xunit;

namespace Tests.Network.DataFormats
{
    public class LoggedFileTests
    {
        [Fact]
        public void TestSerializationToMultipleFileLogs()
        {
            var log1 = new FileLog(Path.GetTempFileName());
            var log2 = new FileLog(Path.GetTempFileName());
            var builder = new LoggedFileBuilder(log1);
            builder.Append(new ByteArray(new byte[] { 0, 1, 2, 3, 4, 5 }));
            var file = builder.Close();
            var channelHint = log2.AddChannel();
            log2.LogEvent(channelHint, new Event(1, EventType.RequestReceived, new Netool.Network.DataEventArgs { Data = file, State = null }, DateTime.Now));
            log2.LogEvent(channelHint, new Event(2, EventType.RequestReceived, new Netool.Network.DataEventArgs { Data = file, State = null }, DateTime.Now));
            using (var reader = log2.CreateReader())
            {
                var e1 = reader.ReadEvent(channelHint, 1);
                var e2 = reader.ReadEvent(channelHint, 2);
                Assert.Equal(file.Length, e1.Data.Data.Length);
                Assert.Equal(3, e1.Data.Data.ReadByte(3));
                // check that it isn't copied repeatedly
                Assert.True(((IEquatable<LoggedFile>)e1.Data.Data).Equals((LoggedFile)e2.Data.Data));
            }

            log1.DeleteFile();
            log2.DeleteFile();
        }
    }
}