using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Acoross.BaseNetworkLib.Async
{
    public class AsyncListenServer<T> where T : IPacketTable, new()
    {
        public Socket m_listenSocket = null;
        public ManualResetEvent m_alldone = new ManualResetEvent(false);

        private Object m_ClientSocketLock = new object();
        private List<ISocket> m_ClientSocketList = new List<ISocket>();
        public bool AddClientSocket(ISocket sock)
        {
            lock(m_ClientSocketLock)
            {
                m_ClientSocketList.Add(sock);
            }

            return true;
        }
        public List<ISocket> GetClientSocketList()
        {
            return m_ClientSocketList;
        }
        public bool SendToAllWithoutMe<T>(ISocket me, Int16 packetNum, T packet) where T : IPacket
        {
            lock(m_ClientSocketLock)
            {
                foreach (ISocket sock in m_ClientSocketList)
                {
                    if (sock == me)
                        continue;

                    PacketHelper.Send(sock, packetNum, packet);
                }
            }

            return true;
        }


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
        // T type 의 IPacketTable 을 사용하는 client Socket 을 만든다.
        public static void AcceptCallback(IAsyncResult ar)
        {
            AsyncListenServer<T> ss = (AsyncListenServer<T>)ar.AsyncState;
            ss.m_alldone.Set();

            Socket listener = ss.m_listenSocket;
            Socket handler = listener.EndAccept(ar);

            Console.WriteLine("connected from {0}", handler.RemoteEndPoint.ToString());
            
            AsyncSocket cliSock = new AsyncSocket(handler, new T());

            ss.AddClientSocket(cliSock);

            cliSock.BeginReceive();
        }
    }
}
