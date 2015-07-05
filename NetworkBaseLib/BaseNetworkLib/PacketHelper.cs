using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

namespace Acoross.BaseNetworkLib
{
    public delegate bool PacketHandler(ISocket sock, byte[] buf);

    public interface IPacketTable
    {
        PacketHandler[] PacketTable();
    }

    public class PacketHelper
    {
        static Int16 m_size = -1;
        public static Int16 Size(IPacket data)
        {
            return (Int16)Marshal.SizeOf(data);
        }

        public const int nHeaderLen = 4;

        public static Int16 ParsePacketNum(byte[] buffer)
        {
            if (buffer.Length < 2)
            {
                return -1;
            }

            return BitConverter.ToInt16(buffer, 0);
        }

        public static Int16 ParseBodyLen(byte[] buffer)
        {
            if (buffer.Length < 4)
            {
                return -1;
            }

            return BitConverter.ToInt16(buffer, 2);
        }

        public static Byte[] GetSendBuffer<T>(Int16 packetNum, T Packet) where T : IPacket
        {
            Byte[] sendbuffer = new Byte[Size(Packet) + nHeaderLen];

            Byte[] btNum = BitConverter.GetBytes(packetNum);
            Buffer.BlockCopy(btNum, 0, sendbuffer, 0, 2);

            Byte[] btSize = BitConverter.GetBytes(Size(Packet));
            Buffer.BlockCopy(btSize, 0, sendbuffer, 2, 2);

            Byte[] buffer = PacketSerializer.SerializeMessage(Packet);
            Buffer.BlockCopy(buffer, 0, sendbuffer, nHeaderLen, Size(Packet));

            return sendbuffer;
        }

        public static bool Send<T>(ISocket sock, Int16 packetNum, T Packet) where T : IPacket
        {
            Byte[] sendbuffer = GetSendBuffer(packetNum, Packet);

            sock.Send(sendbuffer, Size(Packet) + nHeaderLen);

            return true;
        }

        // obsolute
        //public static bool Handle<T>(ISocket sock, byte[] buffer) where T : IPacket
        //{
        //    //T packet = PacketSerializer.DeserializeMsg<T>(buffer, nHeaderLen);
        //    T packet = ParsePacketStruct<T>(buffer);

        //    Int16 readLen = ParseBodyLen(buffer);
        //    if (readLen < Size(packet))
        //    {
        //        Console.WriteLine("read length is too small({0})", readLen);
        //        return true;    // socket close
        //    }

        //    return packet.Handle(sock, buffer);
        //}

        public static T ParsePacketStruct<T>(byte[] buffer) where T : IPacket
        {
            T packet = PacketSerializer.DeserializeMsg<T>(buffer, nHeaderLen);

            return packet;
        }
    }
}
