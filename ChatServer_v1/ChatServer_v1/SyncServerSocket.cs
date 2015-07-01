using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Acoross.Network
{
    class SyncServerSocket
    {
        Socket m_ListenSocket = null;
        Socket m_ClientSocket = null;
        byte[] m_buffer = new byte[1024];
        string data = null;

        // make new m_ListenSocket
        // Listen
        public bool Listen()
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

            m_ListenSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            bool retval = false;

            try
            {
                m_ListenSocket.Bind(localEndPoint);
                m_ListenSocket.Listen(10);

                retval = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return retval;
        }

        public void Receive()
        {
            Console.WriteLine("Waiting for a connection...");

            m_ClientSocket = m_ListenSocket.Accept();

            m_ClientSocket.ReceiveTimeout = 2000;

            Console.WriteLine("socket connected from {0}.", m_ClientSocket.RemoteEndPoint.ToString());

            while (true)
            {
                m_buffer = new byte[1024];
                int bytesRec = 0;

                try
                {
                    bytesRec = m_ClientSocket.Receive(m_buffer);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        Console.WriteLine("timeout");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(e.ToString());
                        m_ClientSocket.Shutdown(SocketShutdown.Both);
                        m_ClientSocket.Close();
                        m_ClientSocket = null;

                        break;
                    }
                }

                // socket disconnected
                if (bytesRec == 0)
                {
                    Console.WriteLine("Socket disconnected.");
                    m_ClientSocket.Shutdown(SocketShutdown.Both);
                    m_ClientSocket.Close();
                    m_ClientSocket = null;
                    return;
                }

                data += Encoding.UTF8.GetString(m_buffer, 0, bytesRec);
                if (data.IndexOf("<EOF>") > 0)
                {
                    Console.WriteLine("Text received : {0}", data);
                    byte[] msg = Encoding.UTF8.GetBytes(data);

                    m_ClientSocket.Send(msg);

                    data = null;
                }
            }
        }

        ~SyncServerSocket()
        {
            if (m_ClientSocket != null)
            {
                m_ClientSocket.Shutdown(SocketShutdown.Both);
                m_ClientSocket.Close();
                m_ClientSocket = null;
                Console.WriteLine("client socket closed.");
            }
        }
    }
}
