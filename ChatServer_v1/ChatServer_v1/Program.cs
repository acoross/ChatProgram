using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer_v1
{
    class Program
    {
        static void Main(string[] args)
        {
            //SyncServerSocket sock = new SyncServerSocket();

            //if (sock.Listen())
            //{
            //    sock.Receive();
            //}
            //else
            //{
            //    Console.WriteLine("Fail to listen.");
            //}

            //AsyncListenServer<CS_PacketTable> instance = (AsyncListenServer<CS_PacketTable>)Activator.CreateInstance(typeof(AsyncListenServer<CS_PacketTable>));
            //instance.StartListening();

            ChatServer.Instance().Run();

            Console.WriteLine("Press any key.");
            Console.Read();
            
            return;
        }
    }
}
