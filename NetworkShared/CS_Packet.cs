using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

using Acoross.BaseNetworkLib;

namespace Acoross.NetworkShared
{
    // client to server packet
    public enum CS_PacketType : short
    {
        CS_LOGIN,
        CS_ECHO,
        // anything
        CS_SAY,
        // s msg
    }

    //public class CS_PacketTable : IPacketTable
    //{
    //    public PacketHandler[] PacketTable()
    //    {
    //        return m_PacketTable;
    //    }

    //    static PacketHandler[] m_PacketTable =
    //    {
    //        PacketHelper.Handle<CS_ECHO_Packet>
    //    };
    //}
    
    // CS_LOGIN
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class CS_LOGIN_Packet : IPacket
    {
        // Data begin
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string pwd;
        // Data end
    }

    // CS_ECHO
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class CS_ECHO_Packet : IPacket
    {
        // Data begin
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string msg;
        // Data end

        //public bool Handle(ISocket sock, byte[] buffer)
        //{
        //    Console.WriteLine("{0}:\"{1}\" from {2}", CS_PacketType.CS_ECHO, msg, sock.GetSocket().RemoteEndPoint.ToString());

        //    // CS_ECHO_Packet 은 SC_ECHO_RP_Packet 과 호환된다... 
        //    PacketHelper.Send(sock, (Int16)SC_PacketType.SC_ECHO_RP, this);

        //    return false;
        //}
    }

    // CS_SAY
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class CS_SAY_Packet : IPacket
    {
        // Data begin
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string msg;
        // Data end

        //// Handle: 받는 측에서의 처리
        //public bool Handle(ISocket sock, byte[] buffer)
        //{
        //    Console.WriteLine("{0}:\"{1}\" from {2}", CS_PacketType.CS_SAY.ToString(), msg, sock.GetSocket().RemoteEndPoint.ToString());

        //    return false;
        //}
    }
}
