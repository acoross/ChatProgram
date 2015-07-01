using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acoross.Network.Async;

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

            ServerSocket_async sock = new ServerSocket_async();
            sock.StartListening();

            Console.WriteLine("Press any key.");
            Console.Read();
            
            return;
        }
    }
}
