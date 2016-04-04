using Netool.Network.DataFormats;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Netool.Network.Tcp
{
    internal class TcpAsyncSender
    {
        private class StopSign : IDataStream
        {
            public static StopSign Instance = new StopSign();
            private StopSign() { }
            long IDataStream.Length { get { throw new NotImplementedException(); } }

            object ICloneable.Clone()
            {
                throw new NotImplementedException();
            }

            byte IDataStream.ReadByte(long index)
            {
                throw new NotImplementedException();
            }

            void IDataStream.ReadBytesToBuffer(byte[] buffer, long start, int length, int offset)
            {
                throw new NotImplementedException();
            }
        }

        private BlockingCollection<IDataStream> queue = new BlockingCollection<IDataStream>();
        public delegate void SendSuccessfulDelegate(IDataStream s);
        public delegate void SendErrorDelegate(Exception e);
        private SendSuccessfulDelegate success;
        private SendErrorDelegate error;
        private Action stop;
        private Thread worker;
        private Socket socket;

        public TcpAsyncSender(Socket socket, SendSuccessfulDelegate success, SendErrorDelegate error, Action stop)
        {
            this.socket = socket;
            this.success = success;
            this.error = error;
            this.stop = stop;
            this.worker = new Thread(execute);
            this.worker.Start();
        }

        public void Send(IDataStream s)
        {
            queue.Add(s);
        }

        public void Stop()
        {
            queue.Add(StopSign.Instance);
        }

        private void execute()
        {
            while(true)
            {
                var s = queue.Take();
                if(!object.ReferenceEquals(s, StopSign.Instance))
                {
                    if (s == null) continue;
                    try
                    {
                        TcpHelpers.Send(socket, s);
                    }
                    catch(Exception e)
                    {
                        error(e);
                        continue;
                    }
                    success(s);
                }
                else
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    catch (Exception e)
                    {
                        error(e);
                        return;
                    }
                    stop();
                    return;
                }
            }
        }
    }

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