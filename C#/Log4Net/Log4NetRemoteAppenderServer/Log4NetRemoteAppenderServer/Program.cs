using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetRemoteAppenderServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //RemoteLoggingSinkImpl.Instance.RegisterRemotingServerChannel();

            //ServiceHost serviceHost = new ServiceHost(typeof(RemoteDataSinkImpl));
            //serviceHost.Opened += serviceHost_Opened;
            //serviceHost.Open();

            ServiceHost serviceHost = new ServiceHost(typeof(MemoryOptimizedRemoteDataSinkImpl));
            serviceHost.Opened += serviceHost_Opened;
            serviceHost.Open();

            Console.Read();
        }

        static void serviceHost_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("服务启动成功");
        }
    }
}
