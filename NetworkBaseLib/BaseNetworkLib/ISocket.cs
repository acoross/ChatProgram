using System;
using System.Collections.Generic;

using System.Net.Sockets;

namespace Acoross.BaseNetworkLib
{
    public interface IAsyncSocketCallback
    {
        void OnAccepted(ISocket sock);
        void OnClosed(ISocket sock);
    }
    
    public interface ISocket
    {
        void Read();
        void Send(byte[] buffer, int nLen);
        Socket GetSocket();
        IOwner Owner
        {
            get;
            set;
        }
    }
}
