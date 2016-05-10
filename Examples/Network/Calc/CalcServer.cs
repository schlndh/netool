using Netool.Network;
using Netool.Network.DataFormats;
using Netool.Network.Tcp;
using System;

namespace Examples.Network.Calc
{
    [Serializable]
    public class CalcServerChannel : BaseServerChannel, IServerChannel
    {
        private IServerChannel channel;
        public new int ID { get { return channel.ID; } }
        public new string Name { get { return channel.Name; } }

        public CalcServerChannel(IServerChannel c)
        {
            channel = c;
            channel.RequestReceived += channel_RequestReceived;
            channel.ResponseSent += channel_ResponseSent;
            channel.ChannelReady += channel_ChannelReady;
            channel.ChannelClosed += channel_ChannelClosed;
            channel.ErrorOccured += channel_ErrorOccured;
        }

        private void channel_ErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void channel_ChannelClosed(object sender)
        {
            OnChannelClosed();
        }

        private void channel_ChannelReady(object sender)
        {
            OnChannelReady();
        }

        private void channel_ResponseSent(object sender, DataEventArgs e)
        {
            OnResponseSent(e.Data);
        }

        private void channel_RequestReceived(object sender, DataEventArgs e)
        {
            var len = sizeof(char) + 2 * sizeof(double);
            if (e.Data.Length >= len)
            {
                var buffer = e.Data.ReadBytes(0, len);
                char oper = BitConverter.ToChar(buffer, 0);
                double op1 = BitConverter.ToDouble(buffer, sizeof(char));
                double op2 = BitConverter.ToDouble(buffer, sizeof(char) + sizeof(double));
                OnRequestReceived(new DataFormats.Calc.CalcData(op1, op2, oper));
            }
        }

        public void Send(IDataStream response)
        {
            channel.Send(response);
        }

        public void Close()
        {
            channel.Close();
        }
    }

    [Serializable]
    public class CalcServer : BaseInstance, IServer
    {
        private TcpServer server;
        public bool IsStarted { get { return server.IsStarted; } }
        public object Settings { get { return server.Settings; } }

        [field: NonSerialized]
        public event EventHandler<IServerChannel> ChannelCreated;

        public CalcServer(TcpServerSettings settings)
        {
            server = new TcpServer(settings);
            server.ChannelCreated += server_ChannelCreated;
            server.ErrorOccured += server_ErrorOccured;
        }

        private void server_ErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void server_ChannelCreated(object sender, IServerChannel e)
        {
            var channel = new CalcServerChannel(e);
            var ev = ChannelCreated;
            if (ev != null)
            {
                ev(this, channel);
            }
        }

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }
    }
}