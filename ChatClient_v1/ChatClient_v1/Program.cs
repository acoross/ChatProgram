using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace ChatClient_v1
{
    class MyClientSocket
    {
        Socket m_Socket = null;
        public byte[] m_buffer = new byte[1024];
        public int m_bytesRecv = 0;

        public void ConnectTo(string ip)
        {
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
                    return;
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

        public void SendAndRecv_Sync(string input)
        {
            if (m_Socket != null)
            {
                try
                {
                    //byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                    byte[] msg = Encoding.ASCII.GetBytes(input + "<EOF>");

                    int bytesSent = m_Socket.Send(msg);
                    m_bytesRecv = m_Socket.Receive(m_buffer);

                    //Console.WriteLine("Echoed test = {0}",
                    //    Encoding.ASCII.GetString(m_buffer, 0, bytesRec));
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
        }

        public static void StartClient(string ip)
        {
            byte[] bytes = new byte[1024];

            while (true)
            {
                string input = Console.ReadLine();

                if (input.Length == 1 && input[0] == 'q')
                {
                    break;
                }

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
                        return;
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


                        //byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                        byte[] msg = Encoding.ASCII.GetBytes(input + "<EOF>");

                        int bytesSent = sender.Send(msg);

                        int bytesRec = sender.Receive(bytes);
                        Console.WriteLine("Echoed test = {0}",
                            Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
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
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length < 3)
            //{
            //    Console.WriteLine("ChatClient ip id pwd");
            //    return;
            //}

            //string ip = args[1];

            string ip = Dns.GetHostName();

            //MyClientSocket.StartClient(ip);

            MyClientSocket sock = new MyClientSocket();

            sock.ConnectTo(ip);
            
            while (true)
            {
                string input = Console.ReadLine();

                if (input.Length == 1 && input[0] == 'q')
                {
                    sock.SendAndRecv_Sync("<EOF>");
                    sock.Disconnect();
                    break;
                }

                sock.SendAndRecv_Sync(input);
                Console.WriteLine(Encoding.ASCII.GetString(sock.m_buffer, 0, sock.m_bytesRecv));
            }
        }
    }
}
