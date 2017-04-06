using System;
using System.CodeDom;
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
            AsyncServer server = new AsyncServer(1000, 4096);
            server.Init();
            server.Start(GetIPEndPoint());

        }

        private static IPEndPoint GetIPEndPoint()
        {
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1569);

            return ipEndPoint;
        }
    }
}
