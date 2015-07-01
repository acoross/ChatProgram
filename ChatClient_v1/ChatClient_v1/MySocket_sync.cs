using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

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

        public bool SendAndRecv_Sync(string input)
        {
            bool retval = false;

            if (m_Socket != null)
            {
                try
                {
                    byte[] header = BitConverter.GetBytes((Int16)1);
                    byte[] msg = Encoding.UTF8.GetBytes(input + "<EOF>");
                    byte[] buffer = new byte[1024];

                    System.Buffer.BlockCopy(header, 0, buffer, 0, 2);
                    buffer[2] = (byte)msg.Length;
                    System.Buffer.BlockCopy(msg, 0, buffer, 3, msg.Length);

                    int bytesSent = m_Socket.Send(buffer, 3 + msg.Length, 0);

                    m_bytesRecv = m_Socket.Receive(m_buffer);
                    // 아래의 경우 상대편 socket 이 close 된 것이다.
                    if (m_bytesRecv == 0)
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                        m_Socket.Close();
                        m_Socket = null;

                        Console.WriteLine("disconnected.");

                        retval = false;
                    }
                    else
                    {
                        retval = true;
                    }
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
