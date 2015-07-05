using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Acoross.NetworkShared;
using Acoross.BaseNetworkLib;

namespace Acoross.Network
{
    class MyClientSocket
    {
        Socket m_Socket = null;
        public byte[] m_buffer = new byte[1024];
        public int m_bytesRecv = 0;

        public bool ConnectTo(string ip)
        {
            bool retval = false;

            // Socket에 연결!
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
                IPAddress[] ipv4Addresses = Array.FindAll(
                    ipHostInfo.AddressList,
                    a => a.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4Addresses.Length <= 0)
                {
                    Console.WriteLine("no ipv4 address(" + ipHostInfo.ToString() + ")");
                    return false;
                }

                IPEndPoint remoteEP = new IPEndPoint(ipv4Addresses[0], 11000);

                // Create a TCP/IP socket.
                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);
                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    m_Socket = sender;
                    retval = true;
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return retval;
        }

        public void Disconnect()
        {
            if (m_Socket != null)
            {
                // Release the socket.
                Socket s = m_Socket;
                m_Socket = null;

                s.Shutdown(SocketShutdown.Both);
                s.Close();
            }
        }

        public bool Send(Int16 nPacketNum, IPacket packet)
        {
            bool retval = false;

            if (m_Socket != null)
            {
                try
                {
                    //byte[] buffer = PacketSerializer.SerializeMessage(packet);
                    Byte[] buffer = PacketHelper.GetSendBuffer(nPacketNum, packet);

                    int bytesSent = m_Socket.Send(buffer, 0);

                    retval = true;
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        Console.WriteLine("connection Aborted (e).");
                    }
                    else
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            return retval;
        }

        public bool Read()
        {
            m_bytesRecv = m_Socket.Receive(m_buffer);
            // 아래의 경우 상대편 socket 이 close 된 것이다.
            if (m_bytesRecv == 0)
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
                m_Socket = null;

                Console.WriteLine("disconnected.");

                return false;
            }
            else
            {
                return true;
            }
        }

        ~MyClientSocket()
        {
            if (m_Socket != null)
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
                m_Socket = null;
            }
        }
    }
}
