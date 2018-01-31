using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class TextLogger : ILog
    {
        private Object Lock = new object();
        string path = "D:/Everything Code/Code Fam/ChatroomFinal/chatLog.txt";        

        public void JoinChat(string userName)
        {
            lock(Lock)
            {
                using (StreamWriter write = File.AppendText(path))
                {
                    write.WriteLine(userName + " has joined the chat at " + DateTime.Now.ToLocalTime());                    
                }
            }
        }

        public void LogMessage(string message)
        {
            lock(Lock)
            {
                using (StreamWriter write = File.AppendText(path))
                {
                    write.WriteLine(message + " at " + DateTime.Now.ToLocalTime());
                }
            }
        }

        public void LeaveChat(string userName)
        {
            lock(Lock)
            {
                using (StreamWriter write = File.AppendText(path))
                {
                    write.WriteLine(userName + " has left the chat at " + DateTime.Now.ToLocalTime());                    
                }
            }
        }

        

        void ILog.JoinChat()
        {
            throw new NotImplementedException();
        }
        void ILog.LogMessage()
        {
            throw new NotImplementedException();
        }
        void ILog.LeaveChat()
        {
            throw new NotImplementedException();
        }
    }
}

