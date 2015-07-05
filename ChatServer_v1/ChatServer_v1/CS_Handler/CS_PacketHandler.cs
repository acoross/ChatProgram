using System;
using System.Collections.Generic;
using System.Text;

using Acoross.NetworkShared;
using Acoross.BaseNetworkLib;

namespace ChatServer_v1.CS_Handler
{
    public class CS_PacketTableNew : IPacketTable
    {
        public PacketHandler[] PacketTable()
        {
            return m_PacketTable;
        }

        static PacketHandler[] m_PacketTable =
        {
            //PacketHelper.Handle<CS_ECHO_Packet>
            CS_Handlers.CS_LOGIN_Handler,
            CS_Handlers.CS_ECHO_Handler,
            CS_Handlers.CS_SAY_Handler,

        };
    }

    public class ChatClientSocketCallback : IAsyncSocketCallback
    {
        // listen socket 에 의해 accept 가 되었을 때 호출된다.
        public void OnAccepted(ISocket sock)
        {
            Console.WriteLine("connected from {0}", sock.GetSocket().RemoteEndPoint.ToString());

            //// client 에게 SC_ACCEPTED
            //SC_ACCEPTED_Packet packet = new SC_ACCEPTED_Packet();
            //packet.AcceptStatus = 1;

            //PacketHelper.Send(sock, (Int16)SC_PacketType.SC_ACCEPTED, packet);
        }

        // socket close 처리 할 때 먼저 호출한다.
        public void OnClosed(ISocket sock)
        {
            // 유저 리스트에서 제거.
            ChatUser owner = (ChatUser)sock.Owner;
            lock(ChatServer.Instance().m_ChatUserMapLock)
            {
                ChatServer.Instance().m_mapChatUser.Remove(owner.name);
            }
        }
    }

    // packet handler
    // public delegate bool PacketHandler(ISocket sock, byte[] buf);

    public class CS_Handlers
    {
        public static bool CS_LOGIN_Handler(ISocket sock, byte[] buffer)
        {
            CS_LOGIN_Packet packet = PacketHelper.ParsePacketStruct<CS_LOGIN_Packet>(buffer);

            Int16 readLen = PacketHelper.ParseBodyLen(buffer);
            if (readLen < PacketHelper.Size(packet))
            {
                Console.WriteLine("read length is too small({0})", readLen);
                return true;    // socket close
            }

            // do login
            UserLoginData iter = ChatServer.Instance().m_UserLoginDataList.Find(x => (x.name == packet.name && x.pwd == packet.pwd));
            if (iter.name == packet.name)
            {
                SC_LOGIN_RESULT_Packet result = new SC_LOGIN_RESULT_Packet();
                
                lock (ChatServer.Instance().m_ChatUserMapLock)
                {
                    ChatUser tmp;
                    if (ChatServer.Instance().m_mapChatUser.TryGetValue(packet.name, out tmp))
                    {
                        result.Result = 0;
                    }
                    else
                    {
                        result.Result = 1;

                        ChatUser newUser = new ChatUser();
                        newUser.name = packet.name;
                        newUser.UserSocket = sock;

                        sock.Owner = newUser;
                        ChatServer.Instance().m_mapChatUser.Add(newUser.name, newUser);
                    }
                }

                Console.WriteLine("{0} logged in.", packet.name);

                PacketHelper.Send(sock, (Int16)SC_PacketType.SC_LOGIN_RESULT, result);
            }
            else
            {
                SC_LOGIN_RESULT_Packet result = new SC_LOGIN_RESULT_Packet();
                result.Result = 0;

                Console.WriteLine("{0} log in failed.({1})", packet.name, sock.GetSocket().RemoteEndPoint.ToString());

                PacketHelper.Send(sock, (Int16)SC_PacketType.SC_LOGIN_RESULT, result);
            }

            return false;
        }

        public static bool CS_ECHO_Handler(ISocket sock, byte[] buffer)
        {
            CS_ECHO_Packet packet = PacketHelper.ParsePacketStruct<CS_ECHO_Packet>(buffer);

            Int16 readLen = PacketHelper.ParseBodyLen(buffer);
            if (readLen < PacketHelper.Size(packet))
            {
                Console.WriteLine("read length is too small({0})", readLen);
                return true;    // socket close
            }

            //return packet.Handle(sock, buffer);
            Console.WriteLine("{0}:\"{1}\" from {2}", CS_PacketType.CS_ECHO, packet.msg, sock.GetSocket().RemoteEndPoint.ToString());

            // CS_ECHO_Packet 은 SC_ECHO_RP_Packet 과 호환된다... 
            PacketHelper.Send(sock, (Int16)SC_PacketType.SC_ECHO_RP, packet);

            return false;
        }

        public static bool CS_SAY_Handler(ISocket sock, byte[] buffer)
        {
            CS_SAY_Packet packet = PacketHelper.ParsePacketStruct<CS_SAY_Packet>(buffer);

            Console.WriteLine("{0}:\"{1}\" from {2}", CS_PacketType.CS_SAY.ToString(), packet.msg, sock.GetSocket().RemoteEndPoint.ToString());

            // 다른 유저들에게 전송
            SC_SAY_Packet sendPacket = new SC_SAY_Packet();
            sendPacket.sender = ((ChatUser)sock.Owner).name;
            sendPacket.msg = packet.msg;

            ChatServer.Instance().SendToAllUserWithoutMe(sock, (Int16)SC_PacketType.SC_SAY, sendPacket);
            
            // 아래는 모든 socket 에게 보내기, 위는 모든 '유저' 에게 보내기
            // 유저는 로그인 성공한 녀석들만 포함.
            //ChatServer.Instance().m_listenServer.SendToAllWithoutMe(sock, (Int16)SC_PacketType.SC_SAY, sendPacket);

            return false;
        }
    }
}
