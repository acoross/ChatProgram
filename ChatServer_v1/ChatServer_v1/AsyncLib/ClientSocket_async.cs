using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Acoross.Network.Async
{
    enum ClientPacket
    {
        CS_LOGIN = 0,
            // s id,
            // s password
        CS_ECHO = 1,
            // anything
    }

    class ClientSocket_async
    {
        public Socket m_Socket = null;
        public const int BufferSize = 1024;
        public byte[] m_buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();

        // 버퍼에 데이터가 들어있는 양.
        public int m_readLen = 0;

        public ClientSocket_async(Socket s)
        {
            m_Socket = s;
        }

        public void BeginReceive()
        {
            m_Socket.BeginReceive(m_buffer, 0, BufferSize - m_readLen, 0, new AsyncCallback(ReadCallback), this);
        }

        public void OnRead(IAsyncResult ar)
        {
            String content = String.Empty;

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
                        if (m_readLen < 3)  // header (2) + Body len (1)
                        {
                            break;
                        }

                        byte nBodyLen = m_buffer[2];
                        if (m_readLen < nBodyLen + 3)
                        {
                            break;
                        }

                        int nPacketHeader = BitConverter.ToInt16(m_buffer, 0);
                        if (nPacketHeader < 0 || nPacketHeader >= ClientPacketHandler.PacketFunctions.Length)   // out of range
                        {
                            Console.WriteLine("packet number is invalid({0})", nPacketHeader);
                            Console.WriteLine("socket closed ({0})", m_Socket.RemoteEndPoint.ToString());

                            m_Socket.Shutdown(SocketShutdown.Both);
                            m_Socket.Close();

                            return;
                        }

                        // packet process - call packet function
                        if (ClientPacketHandler.PacketFunctions[nPacketHeader](this, m_buffer))
                        {
                            Console.WriteLine("socket closed ({0})", m_Socket.RemoteEndPoint.ToString());

                            m_Socket.Shutdown(SocketShutdown.Both);
                            m_Socket.Close();

                            return;
                        }

                        System.Buffer.BlockCopy(m_buffer, m_readLen, m_buffer, 0, BufferSize - m_readLen);
                        m_readLen -= 2;
                        m_readLen -= nBodyLen;

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
            ClientSocket_async ClientSocket = (ClientSocket_async)ar.AsyncState;
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
