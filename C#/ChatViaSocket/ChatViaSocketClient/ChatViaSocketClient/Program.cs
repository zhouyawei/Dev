using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    public static class Program
    {
        public static void Start(string[] args)
        {
            //for (int i = 0; i < _connectionNo; i++)
            //{
            //    AsyncClient asyncClient = new AsyncClient() { Name = "客户端_" + i.ToString() };
            //    ThreadPool.QueueUserWorkItem((x) =>
            //    {
            //        asyncClient.SendTestData();
            //    });

            //}

            _isStop = false;

            if (_clients.Count > 0)
            {
                _clients.Clear();
            }

            for (int i = 0; i < _connectionNo; i++)
            {
                SimpleTCPClient asyncClient = new SimpleTCPClient("客户端_" + i.ToString());
                _clients.Add(asyncClient);

                asyncClient.Initialize();
                asyncClient.RunAsync();
            }

            while (!_isStop)
            {
                foreach (var simpleTcpClient in _clients)
                {
                    simpleTcpClient.SendTestData();
                    
                }

                //Thread.Sleep(TimeSpan.FromTicks(100000));
                Thread.Sleep(500);
            }
            
        }

        public static void Stop()
        {
            _isStop = true;
            foreach (var simpleTcpClient in _clients)
            {
                simpleTcpClient.CloseClientSocket();
            }
        }

        private static List<SimpleTCPClient> _clients = new List<SimpleTCPClient>();
        private static bool _isStop = false;
        private static int _connectionNo = int.Parse(ConfigurationManager.AppSettings["ConnectionNO"]);
    }
}
