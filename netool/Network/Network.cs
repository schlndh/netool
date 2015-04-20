using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Netool.Settings;
namespace Netool.Network
{
    public class ConnectionEventArgs : EventArgs 
    {
        public string ID;
    }
    public class DataEventAgrs : ConnectionEventArgs
    {
        public IByteArrayConvertible Data;
        public object State;
    }
    public delegate void ConnectionCreatedHandler(object sender, ConnectionEventArgs e);
    public delegate void RequestSentHandler(object sender, DataEventAgrs e);
    public delegate void RequestReceivedHandler(object sender, DataEventAgrs e);
    public delegate void ResponseSentHandler(object sender, DataEventAgrs e);
    public delegate void ResponseReceivedHandler(object sender, DataEventAgrs e);
    public delegate void ConnectionClosedHandler(object sender, ConnectionEventArgs e);
    public interface IByteArrayConvertible
    {
        byte[] ToByteArray();
    }
    
    public interface IClientFactory
    {
        IClient CreateClient();
    }
    public interface IServer 
    {
        event ConnectionCreatedHandler ConnectionCreated;
        event RequestReceivedHandler RequestReceived;
        event ResponseSentHandler ResponseSent;
        event ConnectionClosedHandler ConnectionClosed;
        void Start();
        void Stop();
        void Send(string clientID, IByteArrayConvertible response);
        void CloseConnection(string clientID);
    }
    public interface IClient 
    {
        event ConnectionCreatedHandler ConnectionCreated;
        event RequestSentHandler RequestSent;
        event ResponseReceivedHandler ResponseReceived;
        event ConnectionClosedHandler ConnectionClosed;
        void Start();
        void Stop();
        void Send(IByteArrayConvertible request);
    }
    public interface IProxy 
    {
        event ConnectionCreatedHandler ConnectionCreated;
        event RequestReceivedHandler RequestReceived;
        event ResponseSentHandler ResponseSent;
        event ConnectionClosedHandler ConnectionClosed;
        void Start();
        void Stop();
    }
    public static class SocketHelpers
    {
        public static byte[] ReceiveAllFromSocket(Socket socket, int bufferSize = 1024)
        {
            var buffers = new List<byte[]>();
            int bytesRead = 0;
            do
            {
                buffers.Add(new byte[bufferSize]);
                bytesRead = socket.Receive(buffers[buffers.Count - 1]);
            } while (socket.Available > 0 && bytesRead > 0);
            // omit empty bytes in last buffer
            int retLength = (buffers.Count - 1) * bufferSize + bytesRead;
            byte[] ret = new byte[retLength];
            for (int i = 0; i < buffers.Count - 1; ++i)
            {
                buffers[i].CopyTo(ret, bufferSize * i);
            }
            Array.Copy(buffers[buffers.Count - 1], 0, ret, (buffers.Count - 1) * bufferSize, bytesRead);
            return ret;
        }
    }
}
