using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Netool.Settings;
using Netool.Network.DataFormats;
namespace Netool.Network.Tcp
{
    public class TcpClientSettings : BaseSettings
    {
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
    }
    class TcpClient: IClient
    {
        protected TcpClientSettings settings;
        protected Socket socket;
        protected object state;
        protected volatile bool stopped = true;

        public event ConnectionCreatedHandler ConnectionCreated;
        public event RequestSentHandler RequestSent;
        public event ResponseReceivedHandler ResponseReceived;
        public event ConnectionClosedHandler ConnectionClosed;
        public TcpClient(TcpClientSettings settings) 
        {
            this.settings = settings;
        }
        public virtual void Start()
        {
            if(stopped)
            {
                stopped = false;
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(settings.RemoteEndPoint);
                OnConnectionCreated();
                scheduleNextReceive();
            }
        }
        public virtual void Stop()
        {
            if(!stopped)
            {
                stopped = true;
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (ObjectDisposedException e)
                {
                    // already closed
                    return;
                }

                OnConnectionClosed();
            } 
        }
        public virtual void Send(IByteArrayConvertible request)
        {
            try 
            {
                socket.Send(request.ToByteArray());
            }
            catch(ObjectDisposedException e)
            {
                return;
            }
            
            OnRequestSent(request);
        }
        protected virtual void scheduleNextReceive()
        {
            var s = new ReadStateObject(socket, 1024);
            try 
            {
                socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleResponse, s);
            }
            catch (ObjectDisposedException e) { }
            
        }
        protected virtual void handleResponse(IAsyncResult ar)
        {
            var s = (ReadStateObject)ar.AsyncState;
            int bytesRead = 0;
            try 
            {
                bytesRead = socket.EndReceive(ar);
            }
            catch(ObjectDisposedException e)
            {
                // closed
                return;
            }
            if (bytesRead > 0)
            {
                var response = processResponse(s.Buffer, bytesRead);
                OnResponseReceived(response);
                scheduleNextReceive();
            }
            else 
            {
                Stop();
            }
        }
        protected virtual IByteArrayConvertible processResponse(byte[] response, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(response,arr, length);
            return new ByteArray(arr);
        }
        protected virtual void OnConnectionCreated()
        {
            if (ConnectionCreated != null) ConnectionCreated(this, new ConnectionEventArgs { ID = "" });
        }
        protected virtual void OnRequestSent(IByteArrayConvertible request)
        {
            if (RequestSent != null) RequestSent(this, new DataEventAgrs { ID = "", Data = request, State = state });
        }
        protected virtual void OnResponseReceived(IByteArrayConvertible response)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventAgrs { ID = "", Data = response, State = state });
        }
        protected virtual void OnConnectionClosed()
        {
            if (ConnectionClosed != null) ConnectionClosed(this, new ConnectionEventArgs { ID = "" });
        }
    }
}
