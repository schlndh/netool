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
    public delegate void RequestDroppedHandler(object sender, DataEventAgrs e);
    public delegate void ResponseSentHandler(object sender, DataEventAgrs e);
    public delegate void ResponseReceivedHandler(object sender, DataEventAgrs e);
    public delegate void ResponseDroppedHandler(object sender, DataEventAgrs e);
    public delegate void ConnectionClosedHandler(object sender, ConnectionEventArgs e);
    /// <summary>
    /// interface for request/response modification callbacks for proxy
    /// </summary>
    /// <param name="id">client id</param>
    /// <param name="data"></param>
    /// <returns>new data or null if request/response is to be dropped</returns>
    public delegate IByteArrayConvertible DataModifier(string id, IByteArrayConvertible data);
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
        event RequestSentHandler RequestSent;
        event RequestDroppedHandler RequestDropped;
        event ResponseReceivedHandler ResponseReceived;
        event ResponseSentHandler ResponseSent;
        event ResponseDroppedHandler ResponseDropped;
        event ConnectionClosedHandler ConnectionClosed;
        DataModifier RequestModifier { get; set; }
        DataModifier ResponseModifier { get; set; }
        void Start();
        void Stop();
    }
}
