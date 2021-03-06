﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ChatViaSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AsyncServer server = new EchoAsyncServer(_maxClientNum);
                server.Init();
                server.Start(GetIPEndPoint());
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Program->Main出现异常, Exception = {0}", e);
            }
        }

        private static IPEndPoint GetIPEndPoint()
        {
            var listenPort = int.Parse(_listenPort);
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, listenPort);

            return ipEndPoint;
        }

        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string _listenPort = ConfigurationManager.AppSettings["ListenPort"];
        private static int _maxClientNum = int.Parse(ConfigurationManager.AppSettings["MaxClientNum"]);
    }
}
