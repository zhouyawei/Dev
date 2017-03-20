using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int remotePort = 1569;
            string remoteIPInString = "127.0.0.1";
            EndPoint remotEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPInString), remotePort);
            clientSocket.Connect(remotEndPoint);

            byte[] buffer = new byte[4096];
            string content = "Hello Danting";
            byte[] messagesInBytes = Encoding.UTF8.GetBytes(content);
            Array.Copy(messagesInBytes, buffer, messagesInBytes.Length);
            int bytesSend = clientSocket.Send(buffer, messagesInBytes.Length, SocketFlags.None);

            clientSocket.Close();
        }
    }
}
