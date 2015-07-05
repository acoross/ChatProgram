using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

using Acoross.BaseNetworkLib;

namespace Acoross.NetworkShared
{
    // server to client packet
    public enum SC_PacketType : short
    {
        SC_ECHO_RP,
        SC_SAY,
    }

    //public class SC_PacketTable : IPacketTable
    //{
    //    public PacketHandler[] PacketTable()
    //    {
    //        return m_PacketTable;
    //    }

    //    static PacketHandler[] m_PacketTable =
    //    {
    //        PacketHelper.Handle<SC_ECHO_RP_Packet>
    //    };
    //}

    // SC_ECHO_RP
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class SC_ECHO_RP_Packet : IPacket
    {
        // Data begin
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string msg;
        // Data end

        // obsolute
        //public bool Handle(ISocket sock, byte[] buffer)
        //{
        //    Console.WriteLine("\"echo({0})\"", msg);
            
        //    return false;
        //}
    }

    // SC_SAY
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class SC_SAY_Packet : IPacket
    {
        // Data begin
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string sender;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string msg;
        // Data end
    }
}
