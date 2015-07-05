using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public static ManualResetEvent m_alldone = new ManualResetEvent(false);
        public static int loginResult = 0;

        static void AsyncConnection(string ip)
        {
            if (ip == null)
            {
                ip = System.Net.Dns.GetHostName();
            }

            AsyncSocket chatserver = new AsyncSocket(null, new SC_PacketTable(), null);

            if (chatserver.ConnectTo(ip))
            {
                //chatserver.BeginReceive();

                while (true)
                {
                    // send id and pwd
                    Console.Write("login as: ");
                    string input_id = Console.ReadLine();
                    Console.Write("{0}@{1}'s password: ", input_id, chatserver.GetSocket().RemoteEndPoint.ToString());
                    string input_pwd = Console.ReadLine();

                    CS_LOGIN_Packet loginPacket = new CS_LOGIN_Packet();
                    int a = input_id.Length;
                    loginPacket.name = input_id;
                    loginPacket.pwd = input_pwd;
                    PacketHelper.Send(chatserver, (Int16)CS_PacketType.CS_LOGIN, loginPacket);
                    byte[] buffer = new byte[1024];

                    //m_alldone.WaitOne();
                    //if (loginResult == 1)
                    //{
                    //    break;
                    //}
                    chatserver.GetSocket().Receive(buffer);
                    SC_LOGIN_RESULT_Packet result = PacketHelper.ParsePacketStruct<SC_LOGIN_RESULT_Packet>(buffer);
                    if (result.Result == 1)
                    {
                        break;
                    }
                }
                
                chatserver.BeginReceive();
                
                while (true)
                {
                    Console.Write(">> ");
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
            
            //ip = "192.168.0.7";
            //ip = "acoross3.iptime.org";
            AsyncConnection(ip);

            Console.WriteLine("Press any key.");
            Console.Read();
            return;
        }
    }
}
