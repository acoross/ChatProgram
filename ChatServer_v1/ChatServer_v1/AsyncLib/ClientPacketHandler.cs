using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acoross.Network.Async;

namespace Acoross.Network
{
    class ClientPacketHandler
    {
        public delegate bool ClientPacketFunction(ClientSocket_async socket, byte[] packet);

        public static ClientPacketFunction[] PacketFunctions = 
        {
            LogInPacket,    // CS_LOGIN
            Echo,           // CS_ECHO
        };

        // CS_ECHO
        public static bool Echo(ClientSocket_async socket, byte[] packet)
        {
            byte nLen = packet[2];
            string msg = Encoding.UTF8.GetString(packet, 3, nLen);

            Console.WriteLine("\"{0}\" from {1}", msg, socket.m_Socket.RemoteEndPoint.ToString());

            socket.m_Socket.Send(packet, 3, nLen, 0);

            return false;
        }

        // CS_LOGIN
        public static bool LogInPacket(ClientSocket_async socket, byte[] packet)
        {
            return false;   // normal end. -- not close
        }
    }
}
