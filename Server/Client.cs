using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        private NetworkStream stream;
        Object MessageLock = new Object();
        TcpClient client;
        public string UserId;
        public string userName = "";
        public Dictionary<int, Client> userInfo = new Dictionary<int, Client>();

        public NetworkStream Stream
        {
            get
            {
                return stream;
            }
            set
            {
                stream = value;
            }
        } 
        public Client(NetworkStream NewStream, TcpClient Client)
        {
            this.stream = NewStream;
            client = Client;
            this.userName = Receive().Substring(1);
            UserId = "495933b6-1762-47a1-b655-483510072e73";
        }
        public void Send(string Message)
        { 
            byte[] message = Encoding.ASCII.GetBytes(Message);
            Stream.Write(message, 0, message.Count());
        }
        public string Receive()
        {
            bool isReceiving = false;
            string disconnected = " disconnected.";

            if (!isReceiving)
            {
                lock (MessageLock)
                {
                    try
                    {
                        isReceiving = true;
                        byte[] receivedMessage = new byte[256];
                        Stream.Read(receivedMessage, 0, receivedMessage.Length);
                        receivedMessage = ArrayWrapper(receivedMessage);
                        string receivedMessageString = Encoding.ASCII.GetString(receivedMessage);
                        string timeStamp = DateTime.Now.ToString();
                        string recievedMessageToPost = this.userName + "~" + receivedMessageString;
                        Console.WriteLine(timeStamp + recievedMessageToPost);
                        isReceiving = false;
                        return recievedMessageToPost;
                    }
                    catch
                    {
                        Console.WriteLine(" THIS IS THE ERROR");
                        isReceiving = false;
                        return disconnected;
                    }
                }
            }
            return null;
        }
        public static byte[] ArrayWrapper(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }
    }        
}
