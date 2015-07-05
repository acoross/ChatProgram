using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Acoross.BaseNetworkLib.Async
{
    public class AsyncSocket : ISocket
    {
        public Socket m_Socket = null;
        public const int BufferSize = 1024;
        public byte[] m_buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();

        // 버퍼에 데이터가 들어있는 양.
        public int m_readLen = 0;

        IPacketTable m_PacketTable;

        public AsyncSocket(Socket s, IPacketTable myTable)
        {
            m_Socket = s;
            m_PacketTable = myTable;
        }

        // client socket 으로 사용할 시 호출.
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

        public void Read()
        {
            BeginReceive();
        }

        public void Send(byte[] packet, int nLen)
        {
            if (m_Socket != null)
            {
                m_Socket.Send(packet, nLen, 0);
            }
        }

        public Socket GetSocket()
        {
            return m_Socket;
        }

        public void BeginReceive()
        {
            m_Socket.BeginReceive(m_buffer, 0, BufferSize - m_readLen, 0, new AsyncCallback(ReadCallback), this);
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

        public void OnRead(IAsyncResult ar)
        {
            String content = String.Empty;

            if (m_Socket == null)
            {
                Console.WriteLine("OnRead... socket is null.");
                return;
            }

            try
            {
                int bytesRead = m_Socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //sb.Append(Encoding.UTF8.GetString(
                    //    m_buffer, 0, bytesRead));

                    //content = sb.ToString();
                    //if (content.IndexOf("<EOF>") > -1)
                    //{
                    //    Console.WriteLine("Read {0} bytes from socket. \n Data: {1}", content.Length, content);

                    //    // sync send...
                    //    // 임시로
                    //    // echo...
                    //    m_Socket.Send(m_buffer, bytesRead, 0);
                    //    sb.Clear();
                    //}

                    m_readLen += bytesRead;

                    while (true)
                    {
                        if (m_readLen < PacketHelper.nHeaderLen)
                        {
                            break;  // read more
                        }

                        Int16 nPacketNum = PacketHelper.ParsePacketNum(m_buffer);
                        if (nPacketNum < 0 || nPacketNum >= m_PacketTable.PacketTable().Length) // out of range
                        {
                            Console.WriteLine("packet number is invalid({0})", nPacketNum);
                            Console.WriteLine("socket closed ({0})", m_Socket.RemoteEndPoint.ToString());

                            m_Socket.Shutdown(SocketShutdown.Both);
                            m_Socket.Close();
                            m_Socket = null;

                            return;
                        }

                        Int16 nBodyLen = PacketHelper.ParseBodyLen(m_buffer);
                        if (nBodyLen < 0)
                        {
                            Console.WriteLine("body length is invalid({0})", nBodyLen);
                            Console.WriteLine("socket closed ({0})", m_Socket.RemoteEndPoint.ToString());

                            m_Socket.Shutdown(SocketShutdown.Both);
                            m_Socket.Close();
                            m_Socket = null;

                            return;
                        }

                        if (m_readLen < nBodyLen + PacketHelper.nHeaderLen)
                        {
                            break;  // read more
                        }

                        // packet process - call packet function
                        if (m_PacketTable.PacketTable()[nPacketNum](this, m_buffer))    // CPacket.Handle<T>
                        {
                            Console.WriteLine("socket closed ({0})", m_Socket.RemoteEndPoint.ToString());

                            m_Socket.Shutdown(SocketShutdown.Both);
                            m_Socket.Close();
                            m_Socket = null;

                            return;
                        }

                        // buffer 정리
                        System.Buffer.BlockCopy(m_buffer, m_readLen, m_buffer, 0, BufferSize - m_readLen);
                        m_readLen -= nBodyLen;
                        m_readLen -= PacketHelper.nHeaderLen;

                        if (m_readLen == 0)
                        {
                            break;
                        }
                    }

                    // read more
                    BeginReceive();
                }
                else if (bytesRead == 0)
                {
                    Console.WriteLine("socket closed ({0})", m_Socket.RemoteEndPoint.ToString());

                    m_Socket.Shutdown(SocketShutdown.Both);
                    m_Socket.Close();
                    m_Socket = null;

                    return;
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }
        }

        // static
        // AsyncCallback
        public static void ReadCallback(IAsyncResult ar)
        {
            AsyncSocket ClientSocket = (AsyncSocket)ar.AsyncState;
            Socket socket = ClientSocket.m_Socket;

            try
            {
                ClientSocket.OnRead(ar);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
                Console.WriteLine("Exception occured from {0}", socket.RemoteEndPoint.ToString());
            }
        }
    }
}
