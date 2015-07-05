using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acoross.NetworkShared;
using Acoross.BaseNetworkLib;

namespace ChatClient_v1.SC_Handler
{
    public class SC_PacketTable : IPacketTable
    {
        public PacketHandler[] PacketTable()
        {
            return m_PacketTable;
        }

        static PacketHandler[] m_PacketTable =
        {
            SC_Handlers.SC_ECHO_RP_Handler,
            SC_Handlers.SC_SAY_Handler
        };
    }

    // packet handler
    // public delegate bool PacketHandler(ISocket sock, byte[] buf);

    public class SC_Handlers
    {
        public static bool SC_ECHO_RP_Handler(ISocket sock, byte[] buffer)
        {
            SC_ECHO_RP_Packet packet = PacketHelper.ParsePacketStruct<SC_ECHO_RP_Packet>(buffer);

            Int16 readLen = PacketHelper.ParseBodyLen(buffer);
            if (readLen < PacketHelper.Size(packet))
            {
                Console.WriteLine("read length is too small({0})", readLen);
                return true;    // socket close
            }

            Console.WriteLine("\"echo({0})\"", packet.msg);

            return false;
        }

        public static bool SC_SAY_Handler(ISocket sock, byte[] buffer)
        {
            SC_SAY_Packet packet = PacketHelper.ParsePacketStruct<SC_SAY_Packet>(buffer);

            Int16 readLen = PacketHelper.ParseBodyLen(buffer);
            if (readLen < PacketHelper.Size(packet))
            {
                Console.WriteLine("read length is too small({0})", readLen);
                return true;    // socket close
            }

            Console.WriteLine("{0}: {1}", packet.sender, packet.msg);

            return false;
        }
    }
}
