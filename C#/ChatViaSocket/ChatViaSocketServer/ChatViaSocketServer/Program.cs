using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
            AsyncServer server = new EchoAsyncServer(1000);
            server.Init();
            server.Start(GetIPEndPoint());
        }

        private static IPEndPoint GetIPEndPoint()
        {
            var listenPort = int.Parse(_listenPort);
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, listenPort);

            return ipEndPoint;
        }

        private static string _listenPort = ConfigurationManager.AppSettings["ListenPort"];
    }
}
