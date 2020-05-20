using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    class Program
    {
        public static ManualResetEvent allDone = new ManualResetEvent(true);
        public static List<Client> Clients = new List<Client>();
        static Random random = new Random();

        private static byte[] answ = new byte[50];
        static string pass;
        static char[] cry =
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '1','2','3','4','5','6','7','8','9','0','_','@','%','$','#','/','*','?','^','&','!','=','-','|','$','='
        };
        public int ESTAESUNAVARIABLECOCHINA_DE_PRUEBA;
        static void Main(string[] args)
        {

            Thread t = new Thread(new ThreadStart(StartListening));
            t.Start();
            WannaCry();

        }

        static void StartListening()
        {
            Console.WriteLine("Initializing...\n");
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 443);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                listener.Bind(endPoint);
                listener.Listen(10);
                Console.WriteLine("\nDone!\n");
                while (true)
                {

                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            allDone.Set();

            handler.Receive(answ);
            if (Encoding.ASCII.GetString(answ) != pass)
            {
                Console.WriteLine("Connection:\n     [ADDRESS ]: {0}\n     [PASSWORD]: {1}\n     [STATE]: REJECTED/CLOSED\n", handler.RemoteEndPoint, Encoding.ASCII.GetString(answ));
                handler.Send(Encoding.UTF8.GetBytes("0"));
                handler.Disconnect(true);

            }
            else
            {
                Console.WriteLine("Connection:\n     [ADDRESS ]: {0}\n     [PASSWORD]: {1}\n     [STATE   ]: ACCEPTED/CONNECTED\n", handler.RemoteEndPoint, Encoding.ASCII.GetString(answ));
                handler.Send(Encoding.UTF8.GetBytes("1"));
                Client c = new Client(handler);
                Clients.Add(c);
                handler.BeginReceive(c.buffer, 0, 1024, 0, new AsyncCallback(c.ReadCallback), c.socket);
            }

        }



        public static void SendMsg(Socket s, byte[] buffer)
        {
            foreach (Client c in Clients)
            {
                if (s != c.socket)
                {
                    try
                    {
                        if (c != null)
                        {
                            c.socket.Send(buffer);
                        }
                        else
                        {
                            Clients.Remove(c);
                        }
                    }
                    catch
                    {
                        Clients.Remove(c);
                    }
                }
            }
        }

        static void WannaCry()
        {
            for (int i = 0; i < 50; i++)
            {
                pass += cry[random.Next(0, cry.Length - 1)];
            }
            Console.WriteLine("[DEBUG] PASSWORD -> {0}", pass);
        }

    }
}
