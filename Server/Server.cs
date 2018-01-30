using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server : TextLogger
    {
        Client client;
        public Dictionary<int, Client> users = new Dictionary<int, Client>();
        TcpListener listener;
        private Queue<string> queueMessages;
        private Object Lock = new object();
        ILog textLogger;
        private string disconnected = " disconnected.";
        private string saveUserName;
        string message = null;
        string previousMessage = null;
        private static bool serverStatus;
        List<Client> clientListeners = new List<Client>();
        List<Thread> perUserListenerThreads = new List<Thread>();

        public static bool IsServerUp
        {
            get
            {
                return serverStatus;
            }
            set
            {
                serverStatus = value;
            }
        }
        public void Run()
        {
            IsServerUp = true;
            Task.Run(() => AcceptClient());
        }

        private void AcceptClient()
        {
            int UserId = 0;
            bool ThreadPerUserBalance = false;

            while (serverStatus)
            {
                TcpClient clientSocket = default(TcpClient);
                clientSocket = listener.AcceptTcpClient();
                Console.WriteLine("Connection Initiated");
                NetworkStream stream = clientSocket.GetStream();
                client = new Client(stream, clientSocket);
                lock (Lock) users.Add(UserId, client);
                clientListeners.Add(client);
                Task.Run(() => WorldShoutNewUser(clientListeners[clientListeners.Count - 1].userName.ToString()));
                UserId += 1;
                if (!ThreadPerUserBalance)
                {
                    ThreadPerUserBalance = true;
                    Task.Run(() => CreateThreadsForReceive());
                }
            }
        }

        public void CreateThreadsForReceive()
        {
            int i = 0;
            while (serverStatus)
            {
                if (perUserListenerThreads.Count < clientListeners.Count)
                {
                    for (i = perUserListenerThreads.Count; i < clientListeners.Count; i++)
                    {
                        Thread listener = new Thread(new ThreadStart(Receive));
                        perUserListenerThreads.Add(listener);
                        listener.Start();
                    }
                }
            }
        }

        public void Broadcast(string sendMessage)
        {
            lock (Lock)
            {
                for (int i = 0; i < clientListeners.Count; i++)
                {
                    //while()
                
                    try
                    {
                        RemoveFromQueue();                        
                        clientListeners[i].Send(sendMessage);
                        LogMessage(sendMessage);
                        
                    }
                    catch
                    {
                        Console.WriteLine(clientListeners[i].userName + " has disconnected");
                        clientListeners[i].Stream.Close();
                        users.Remove(i);
                        clientListeners.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void RemoveFromQueue()
        {
            queueMessages.Dequeue();
        }

        public void Receive()
        { 
            int listenerCount = 0;
            int storeIndexCount = 0;

            if (message == null || message == previousMessage)
            {
                listenerCount += 1;
                if (listenerCount >= clientListeners.Count)
                {
                    listenerCount = 0;
                }
                if (message != disconnected)
                {
                    message = clientListeners[listenerCount].Receive();
                    AddToQueue(message);
                }
            }
            lock (Lock)
            {
                if (message == disconnected)
                {
                    storeIndexCount = listenerCount;
                    saveUserName = clientListeners[storeIndexCount].userName;
                    Broadcast(saveUserName + disconnected);
                    message = null;
                    AddToQueue(message);
                    LeaveChat(saveUserName);
                }
            }
            if (message != null)
            {
                previousMessage = message;
                lock (Lock)
                {
                    AddToQueue(message);
                    Broadcast(message);
                }
                message = null;
            }
            perUserListenerThreads.RemoveAt(0);
        }
        
        public void Respond(string body)
        {
             client.Send(body);            
        }
        public void WorldShoutNewUser(string newUser)
        {
            for (int i = 0; i < clientListeners.Count; i++)
            {
                clientListeners[i].Send(newUser + " has joined the chatroom.");
                AddToQueue(newUser);
                JoinChat(newUser);
            }  
        }
        private void AddToQueue(string message)
        {
            lock(Lock)
            {                
                queueMessages.Enqueue(message);
            }                      
        }

        public Server(ILog logger)
        {
            textLogger = logger;
            queueMessages = new Queue<string>();
            int port = 9999;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }
    }
}
