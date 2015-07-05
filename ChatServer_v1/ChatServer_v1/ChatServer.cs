using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acoross.NetworkShared;
using Acoross.BaseNetworkLib;
using Acoross.BaseNetworkLib.Async;

using ChatServer_v1.CS_Handler;

namespace ChatServer_v1
{
    public class ChatServer
    {
        private ChatServer()
        {}

        public AsyncListenServer<CS_PacketTableNew> m_listenServer = null;
        private Object m_ClientSocketsLock = new Object();
        private List<ISocket> m_ClientSockets = new List<ISocket>();

        public void AddClientSocket(ISocket newSock)
        {
            lock(m_ClientSocketsLock)
            {
                m_ClientSockets.Add(newSock);
            }
        }

        private static ChatServer m_Instance = null;
        public static ChatServer Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new ChatServer();
            }

            return m_Instance;
        }

        public List<ISocket> GetClientSocketList()
        {
            return m_listenServer.GetClientSocketList();
        }

        public void Run()
        {
            m_listenServer = new AsyncListenServer<CS_PacketTableNew>();
            m_listenServer.StartListening();
        }
    }
}
