using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ChatViaWCFServer.Server;
using log4net;

namespace ChatViaWCFServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string msg = string.Empty;
            try
            {
                ServiceHost serviceHost = new ServiceHost(typeof(ChatImpl));
                serviceHost.Opened += (sender, e) =>
                {
                    msg = "ChatViaWCF服务启动成功，按Esc键退出程序...";
                    WriteLog(msg); 
                };
                serviceHost.Closed += (sender, e) =>
                {
                    msg = "ChatViaWCF服务关闭成功...";
                    WriteLog(msg); 
                };

                serviceHost.Open();
                
                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        serviceHost.Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                msg = string.Format("Main方法出现异常，Exception = {0}", e);
                WriteLog(msg);
                Console.Read();
            }
        }

        private static void WriteLog(string msg)
        {
            _log.Info(msg);
            Console.WriteLine(msg);
        }

        private static ILog _log = LogManager.GetLogger(typeof(Program));
    }
}
