using Netool.Network.DataFormats;
using System;
using System.Net.Sockets;

namespace Netool.Network.Tcp
{
    static internal class TcpHelpers
    {
        public static void Send(Socket socket, IDataStream stream)
        {
            if (stream.Length < socket.SendBufferSize) socket.Send(stream.ReadBytes());
            else
            {
                var buffer = new byte[socket.SendBufferSize];
                var remaining = stream.Length;
                long start = 0;
                while (remaining > 0)
                {
                    var read = (int)Math.Min(remaining, buffer.Length);
                    stream.ReadBytesToBuffer(buffer, start, read);
                    start += read;
                    remaining -= read;
                    socket.Send(buffer, read, SocketFlags.None);
                }
            }
        }
    }
}