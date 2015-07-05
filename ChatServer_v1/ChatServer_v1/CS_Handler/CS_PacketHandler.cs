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
            CS_Handlers.CS_ECHO_Handler,
            CS_Handlers.CS_SAY_Handler,

        };
    }

    // packet handler
    // public delegate bool PacketHandler(ISocket sock, byte[] buf);

    public class CS_Handlers
    {
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
            sendPacket.sender = "sender";
            sendPacket.msg = packet.msg;

            ChatServer.Instance().m_listenServer.SendToAllWithoutMe(sock, (Int16)SC_PacketType.SC_SAY, sendPacket);

            return false;
        }
    }
}
