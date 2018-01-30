using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        TcpClient clientSocket;
        NetworkStream stream;
        private bool connectionStatus;
        private string username;

        public string Username
        {
            get
            {
                return username;
            }
        }

        public bool IsConnected
        {
            get
            {
                return connectionStatus;
            }
            set
            {
                connectionStatus = value;
            }
        }

        public Client(string IP, int port)
        {
            clientSocket = new TcpClient();
            try
            {
                Console.WriteLine("Welcome to the Chatroom!");
                AskUsername();
                clientSocket.Connect(IPAddress.Parse(IP), port);
                connectionStatus = true;
                Console.WriteLine("You're now connected! Say hello! "+ username +" \n");
                stream = clientSocket.GetStream();
            }
            catch
            {
                connectionStatus = false;
            }
        }
        public void SendName()
        {
            string messageString = Username;
            byte[] message = Encoding.ASCII.GetBytes(messageString);
            stream.Write(message, 0, message.Count());
        }

        Task Send()
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    if (clientSocket.Connected)
                    {
                        string messageString = UI.GetInput();
                        byte[] message = Encoding.ASCII.GetBytes(messageString);
                        stream.Write(message, 0, message.Count());
                    }
                }
            });
        }
        Task Receive()
        {
            return Task.Run(() =>
            {
                Object messageLock = new Object();
                lock (messageLock)
                {
                    if (clientSocket.Connected)
                    {
                        byte[] receivedMessage = new byte[256];
                        stream.Read(receivedMessage, 0, receivedMessage.Length);
                        receivedMessage = ArrayWrapper(receivedMessage);
                        UI.DisplayMessage(Encoding.ASCII.GetString(receivedMessage));
                    }
                }
            });
        }



        //public Task Send()
        //{
        //    return Task.Run(() =>
        //    {
        //        while (IsConnected)
        //        {
        //            try
        //            {
        //                string messageString = UI.GetInput();
        //                byte[] message = Encoding.ASCII.GetBytes(messageString);
        //                stream.Write(message, 0, message.Count());
        //            }
        //            catch
        //            {
        //                IsConnected = false;
        //            }
        //        }
        //    });
        //}
        //public void Receive()
        //{
        //    while (IsConnected)
        //    {
        //        try
        //        {
        //            byte[] receivedMessage = new byte[256];
        //            stream.Read(receivedMessage, 0, receivedMessage.Length);
        //            receivedMessage = ArrayWrapper(receivedMessage);
        //            UI.DisplayMessage(Encoding.ASCII.GetString(receivedMessage));
        //        }
        //        catch
        //        {
        //            IsConnected = false;
        //        }
        //    }
        //}
        public static byte[] ArrayWrapper(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, f => f != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }
        public void AskUsername()
        {
            Console.WriteLine("What's your name?");
            username = Console.ReadLine();
        }

        public void AsynchSendReceive()
        {
            while (true)
            {
                Parallel.Invoke(
                    async () =>
                    {
                        await Send();
                    },
                    async () =>
                    {
                        await Receive();
                    }
                );
            }
        }

    }
}
