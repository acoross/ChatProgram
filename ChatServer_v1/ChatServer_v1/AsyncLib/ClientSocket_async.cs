using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Acoross.Network.Async
{
    class ClientSocket_async
    {
        public Socket m_Socket = null;
        public const int BufferSize = 1024;
        public byte[] m_buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();

        public ClientSocket_async(Socket s)
        {
            m_Socket = s;
        }

        public void BeginReceive()
        {
            m_Socket.BeginReceive(m_buffer, 0, BufferSize, 0, new AsyncCallback(ReadCallback), this);
        }

        // AsyncCallback
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            ClientSocket_async ClientSocket = (ClientSocket_async)ar.AsyncState;
            Socket socket = ClientSocket.m_Socket;

            int bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                ClientSocket.sb.Append(Encoding.UTF8.GetString(
                    ClientSocket.m_buffer, 0, bytesRead));

                content = ClientSocket.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data: {1}", content.Length, content);

                    // sync send...
                    // 임시로
                    socket.Send(ClientSocket.m_buffer, bytesRead, 0);
                    ClientSocket.sb.Clear();

                    socket.BeginReceive(ClientSocket.m_buffer, 0, ClientSocket_async.BufferSize, 0,
                        new AsyncCallback(ReadCallback), ClientSocket);
                }
                else
                {
                    socket.BeginReceive(ClientSocket.m_buffer, 0, ClientSocket_async.BufferSize, 0,
                        new AsyncCallback(ReadCallback), ClientSocket);
                }
            }
        }
    }
}
