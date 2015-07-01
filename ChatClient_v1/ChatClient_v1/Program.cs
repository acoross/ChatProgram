using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acoross.Network;

namespace ChatClient_v1
{
    class Program
    {
        static void SyncSocket()
        {
            string ip = System.Net.Dns.GetHostName();
            MyClientSocket sock = new MyClientSocket();

            if (sock.ConnectTo(ip))
            {
                while (true)
                {
                    string input = Console.ReadLine();

                    if (input.Length == 1 && input[0] == 'q')
                    {
                        sock.Disconnect();
                        break;
                    }

                    if (!sock.SendAndRecv_Sync(input))
                    {
                        break;
                    }
                    string tmp = Encoding.UTF8.GetString(sock.m_buffer, 0, sock.m_bytesRecv);
                    Console.WriteLine(Encoding.UTF8.GetString(sock.m_buffer, 0, sock.m_bytesRecv));
                }
            }
        }

        static void Main(string[] args)
        {
            //if (args.Length < 3)
            //{
            //    Console.WriteLine("ChatClient ip id pwd");
            //    return;
            //}

            //string ip = args[1];

            SyncSocket();

            Console.WriteLine("Press any key.");
            Console.Read();
            return;
        }
    }
}
