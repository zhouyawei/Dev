using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int localPort = 1569;
            EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            serverSocket.Bind(localEndPoint);
            int backlog = 10000;
            serverSocket.Listen(backlog);

            Socket clientSocket = serverSocket.Accept();
            byte[] buffer = new byte[4096];
            int bytesRead = clientSocket.Receive(buffer);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine(message);

            clientSocket.Close();

            serverSocket.Close();

        }
    }
}
