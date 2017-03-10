using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Log4NetWCFAppender.Contract;

namespace Log4NetRemoteAppenderServer
{
    class RemoteDataSinkImpl : IRemoteDataSink
    {
        public void NewLogs(string message)
        {
            Console.WriteLine(message);
        }
    }
}
