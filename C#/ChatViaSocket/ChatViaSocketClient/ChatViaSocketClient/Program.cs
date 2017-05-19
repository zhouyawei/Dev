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
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 500; i++)
            {
                AsyncClient asyncClient = new AsyncClient() { Name = "客户端_" + i.ToString() };
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    asyncClient.SendTestData();
                });

            }

            Console.Read();
        }
    }
}
