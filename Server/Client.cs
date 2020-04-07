using System.Text;
using System.Net.Sockets;
using System;

namespace Server
{
    class Client
    {
        public Socket socket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        string[] fMsg;

        public Client(Socket listener)
        {
            socket = listener;
        }

        public void ReadCallback(IAsyncResult ar)
        {
            Socket so = (Socket)ar.AsyncState;
            Socket handler = so;

            try
            {
                int read = handler.EndReceive(ar);
                if (read > 0)
                {
                    fMsg = Encoding.ASCII.GetString(buffer, 0, read).Split('~');
                    if (read > 0)
                    {
                        if (fMsg[2] != "PARAM_EXIT")
                        {
                            Console.WriteLine("[{1}] MESSAGE -> {0}", fMsg[1], handler.RemoteEndPoint);
                            Program.SendMsg(handler, buffer);
                            handler.BeginReceive(buffer, 0, BufferSize, 0, new AsyncCallback(ReadCallback), handler);
                        }
                        else
                        {
                            Console.WriteLine("[{1}] DISCONNECTED -> {0}", fMsg[1], handler.RemoteEndPoint);
                            Program.SendMsg(handler, buffer);
                            handler.Disconnect(true);
                        }
                    }
                }
            }
            catch
            {
                handler.Disconnect(true);
                Program.Clients.Remove(this);
            }
        }
    }
}
