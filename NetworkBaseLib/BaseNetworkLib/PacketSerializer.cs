using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

namespace Acoross.BaseNetworkLib
{
    public class PacketSerializer
    {
        public static byte[] SerializeMessage(IPacket msg)
        {
            int size = Marshal.SizeOf(msg);
            Byte[] packet = new Byte[size];
            
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(msg, ptr, true);
            Marshal.Copy(ptr, packet, 0, size);
            Marshal.FreeHGlobal(ptr);

            return packet;
        }

        public static T DeserializeMsg<T>(Byte[] data, int offset) where T : IPacket
        {
            int size = Marshal.SizeOf(typeof(T));

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, offset, ptr, size);
            T retStruct = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return retStruct;
        }
    }
}
