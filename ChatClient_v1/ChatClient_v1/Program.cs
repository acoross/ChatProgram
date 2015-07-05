using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acoross.NetworkShared;
using Acoross.Network;
using Acoross.BaseNetworkLib;
using Acoross.BaseNetworkLib.Async;

using ChatClient_v1.SC_Handler;

namespace ChatClient_v1
{
    class Program
    {
        static void AsyncConnection(string ip)
        {
            if (ip == null)
            {
                ip = System.Net.Dns.GetHostName();
            }

            AsyncSocket chatserver = new AsyncSocket(null, new SC_PacketTable());

            if (chatserver.ConnectTo(ip))
            {
                chatserver.BeginReceive();

                while (true)
                {
                    string input = Console.ReadLine();
                    if (input.Length == 1 && input[0] == 'q')
                    {
                        chatserver.Disconnect();
                        break;
                    }

                    //CS_ECHO_Packet echo = new CS_ECHO_Packet();
                    CS_SAY_Packet echo = new CS_SAY_Packet();
                    echo.msg = input;

                    //if (!PacketHelper.Send(chatserver, (Int16)CS_PacketType.CS_ECHO, echo))
                    if (!PacketHelper.Send(chatserver, (Int16)CS_PacketType.CS_SAY, echo))
                    {
                        break;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine("arg({0})", arg);
            }

            string ip = null;
            if (args.Length >= 1)
            {
                ip = args[0];
            }

            AsyncConnection(ip);

            Console.WriteLine("Press any key.");
            Console.Read();
            return;
        }
    }
}
