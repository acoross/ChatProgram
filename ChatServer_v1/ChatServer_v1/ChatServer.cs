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
    public struct UserLoginData
    {
        public string name;
        public string pwd;
    }

    public class ChatServer
    {
        private ChatServer()
        {}

        public List<UserLoginData> m_UserLoginDataList = new List<UserLoginData>();

        public Object m_ChatUserMapLock = new object();
        public Dictionary<string, ChatUser> m_mapChatUser = new Dictionary<string, ChatUser>();

        public AsyncListenServer<CS_PacketTableNew, ChatClientSocketCallback> m_listenServer = null;

        private static ChatServer m_Instance = null;
        public static ChatServer Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new ChatServer();
            }

            return m_Instance;
        }

        public bool SendToAllUserWithoutMe<Ts>(ISocket me, Int16 packetNum, Ts packet) where Ts : IPacket
        {
            lock(m_ChatUserMapLock)
            {
                foreach (ChatUser user in m_mapChatUser.Values)
                {
                    if (user.UserSocket == me)
                        continue;

                    PacketHelper.Send(user.UserSocket, packetNum, packet);
                }
            }

            return true;
        }

        public List<ISocket> GetClientSocketList()
        {
            return m_listenServer.GetClientSocketList();
        }

        public void Run()
        {
            m_listenServer = new AsyncListenServer<CS_PacketTableNew, ChatClientSocketCallback>();
            m_listenServer.StartListening();
        }
    }
}
