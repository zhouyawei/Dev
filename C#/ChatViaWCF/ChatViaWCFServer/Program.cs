using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using ChatViaWCFServer.Server;
using log4net;
using System.Reflection;
using System.Threading;

namespace ChatViaWCFServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;

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
                    //if (Console.ReadKey().Key == ConsoleKey.Escape)
                    //{
                    //    serviceHost.Close();
                    //    break;
                    //}

                    var dispatchers = serviceHost.ChannelDispatchers;
                    foreach (var dispatcher in dispatchers)
                    {
                        var channelDispatcher = dispatcher as ChannelDispatcher;
                        var type = channelDispatcher.GetType();
                        FieldInfo channelsfieldInfo = type.GetField("channels", BindingFlags.Instance | BindingFlags.NonPublic);
                        var communicationObjectManager = channelsfieldInfo.GetValue(channelDispatcher);
                        var type_communicationObjectManager = communicationObjectManager.GetType();
                        var busyCount_FieldInfo = type_communicationObjectManager.BaseType.GetField("busyCount", BindingFlags.Instance | BindingFlags.NonPublic);
                        var busyCount = busyCount_FieldInfo.GetValue(communicationObjectManager);

                        var url = dispatcher.Listener.Uri.ToString();
                        var info = string.Format("Url = {0}, BusyCount = {1}", url, busyCount);
                        Console.WriteLine(info);
                        Thread.Sleep(5000);

                        //if (Console.ReadKey().Key == ConsoleKey.Escape)
                        //{
                        //    serviceHost.Close();
                        //    break;
                        //}
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
