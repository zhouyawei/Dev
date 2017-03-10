using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log4NetMemoryOptimizedAppender.Contract;

namespace Log4NetRemoteAppenderServer
{
    public class MemoryOptimizedRemoteDataSinkImpl : IRemoteDataSink
    {
        public void NewLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}
