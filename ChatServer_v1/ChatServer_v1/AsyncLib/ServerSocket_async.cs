using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Acoross.Network.Async
{
    class ServerSocket_async
    {
        public Socket m_listenSocket = null;
        public ManualResetEvent m_alldone = new ManualResetEvent(false);

        public bool StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] ipv4Addresses = Array.FindAll(
                ipHostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            if (ipv4Addresses.Length <= 0)
            {
                Console.WriteLine("no ipv4 address(" + ipHostInfo.ToString() + ")");
                return false;
            }

            IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], 11000);

            m_listenSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_listenSocket.Bind(localEndPoint);
                m_listenSocket.Listen(100);

                while (true)
                {
                    m_alldone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    m_listenSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        this);

                    m_alldone.WaitOne();
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }

            return false;
        }

        // AsyncCallback
        public static void AcceptCallback(IAsyncResult ar)
        {
            ServerSocket_async ss = (ServerSocket_async)ar.AsyncState;
            ss.m_alldone.Set();

            Socket listener = ss.m_listenSocket;
            Socket handler = listener.EndAccept(ar);

            Console.WriteLine("connected from {0}", handler.RemoteEndPoint.ToString()); 

            ClientSocket_async cliSock = new ClientSocket_async(handler);
            cliSock.BeginReceive();
        }
    }
}
