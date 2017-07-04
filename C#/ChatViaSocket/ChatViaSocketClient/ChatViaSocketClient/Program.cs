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
        public static void Main(string[] args)
        {
            //for (int i = 0; i < _connectionNo; i++)
            //{
            //    AsyncClient asyncClient = new AsyncClient() { Name = "客户端_" + i.ToString() };
            //    ThreadPool.QueueUserWorkItem((x) =>
            //    {
            //        asyncClient.SendTestData();
            //    });

            //}

            List<SimpleTCPClient> clients = new List<SimpleTCPClient>();
            for (int i = 0; i < _connectionNo; i++)
            {
                SimpleTCPClient asyncClient = new SimpleTCPClient("客户端_" + i.ToString());
                clients.Add(asyncClient);

                asyncClient.Initialize();
                asyncClient.RunAsync();
            }

            while (true)
            {
                foreach (var simpleTcpClient in clients)
                {
                    simpleTcpClient.SendTestData();
                    Thread.Sleep(TimeSpan.FromTicks(100));
                }    
            }
            
            Console.Read();
        }

        private static int _connectionNo = int.Parse(ConfigurationManager.AppSettings["ConnectionNO"]);
    }
}
