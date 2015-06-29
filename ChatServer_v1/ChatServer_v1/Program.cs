using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace ChatServer_v1
{
    class ServerSocket
    {
        Socket m_ListenSocket = null;
        Socket m_ClientSocket = null;
        byte[] m_buffer = new byte[1024];
        string data = null;

        public void Listen()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] ipv4Addresses = Array.FindAll(
                ipHostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            if (ipv4Addresses.Length <= 0)
            {
                Console.WriteLine("no ipv4 address(" + ipHostInfo.ToString() + ")");
                return;
            }

            IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], 11000);

            Socket m_ListenSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_ListenSocket.Bind(localEndPoint);
                m_ListenSocket.Listen(10);

                Console.WriteLine("Waiting for a connection...");

                m_ClientSocket = m_ListenSocket.Accept();

                while (true)
                {
                    m_buffer = new byte[1024];
                    int bytesRec = m_ClientSocket.Receive(m_buffer);
                    data += Encoding.ASCII.GetString(m_buffer, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        Console.WriteLine("Text received : {0}", data);
                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        m_ClientSocket.Send(msg);

                        data = null;
                    }
                    else if (data.IndexOf("<EOF>") == 0)
                    {
                        m_ClientSocket.Shutdown(SocketShutdown.Both);
                        m_ClientSocket.Close();

                        break;
                    }
                }       
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    class Program
    {
        public static string data = null;

        public static void StartListening()
        {
            byte[] bytes = new Byte[1024];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] ipv4Addresses = Array.FindAll(
                ipHostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            
            if (ipv4Addresses.Length <= 0)
            {
                Console.WriteLine("no ipv4 address(" + ipHostInfo.ToString() + ")");
                return;
            }

            IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], 11000);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");

                    Socket handler = listener.Accept();
                    data = null;
                    
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        { break; }
                    }
                    Console.WriteLine("Text received : {0}", data);

                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress Enter to continue...");
            Console.Read();
        }

        static void Main(string[] args)
        {
            //StartListening();

            ServerSocket sock = new ServerSocket();

            sock.Listen();

            return;
        }
    }
}
