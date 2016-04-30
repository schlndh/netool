using Netool.Network.DataFormats;
using System;
using System.Net.Sockets;

namespace Netool.Network.Tcp
{
    internal class ReceiveStateObject
    {
        public byte[] Buffer;

        public ReceiveStateObject(int bufferSize)
        {
            Buffer = new byte[bufferSize];
        }
    }

    [Serializable]
    internal class BaseTcpChannel
    {
        [NonSerialized]
        private Action<IDataStream> OnReceived;

        [NonSerialized]
        private Action<IDataStream> OnSent;

        [NonSerialized]
        private Action<Exception> OnErrorOccured;

        [NonSerialized]
        private Action OnChannelClosed;

        [NonSerialized]
        private Socket socket;

        [NonSerialized]
        private object socketLock = new object();

        public int ReceiveBufferSize { get; set; }

        public BaseTcpChannel(Socket socket, Action<IDataStream> onReceived, Action<IDataStream> onSent,
            Action<Exception> onError, Action onClosed, int receiveBufferSize = 8192)
        {
            this.socket = socket;
            this.OnReceived = onReceived;
            this.OnSent = onSent;
            this.OnErrorOccured = onError;
            this.OnChannelClosed = onClosed;
            ReceiveBufferSize = receiveBufferSize;
            // don't call scheduleNextReceive right away, the ChannelCreated event must be raised first
        }

        /// <summary>
        /// Never call this method directly unless when you're creating a object of this class manually,
        /// then call it after raising a ChannelCreated event
        /// </summary>
        public void scheduleNextReceive()
        {
            // this is purposefully using the private method naming convetion
            var s = new ReceiveStateObject(ReceiveBufferSize);
            try
            {
                socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleRequest, s);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception e)
            {
                OnErrorOccured(e);
                Close();
            }
        }

        private void handleRequest(IAsyncResult ar)
        {
            var stateObject = (ReceiveStateObject)ar.AsyncState;
            int bytesRead = 0;
            try
            {
                bytesRead = socket.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                OnErrorOccured(e);
                Close();
            }
            if (bytesRead > 0)
            {
                var processed = processData(stateObject.Buffer, bytesRead);
                OnReceived(processed);
                scheduleNextReceive();
            }
            else
            {
                Close();
            }
        }

        private IDataStream processData(byte[] data, int length)
        {
            return new ByteArray(data, 0, length);
        }

        /// <inheritdoc />
        public void Send(IDataStream data)
        {
            try
            {
                lock (socketLock)
                {
                    TcpHelpers.Send(socket, data);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                OnErrorOccured(e);
                return;
            }
            OnSent(data);
        }

        /// <inheritdoc />
        public void Close()
        {
            try
            {
                lock (socketLock)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                OnErrorOccured(e);
            }
            OnChannelClosed();
        }
    }
}