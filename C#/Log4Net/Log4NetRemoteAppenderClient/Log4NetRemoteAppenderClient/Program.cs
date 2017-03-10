using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Reflection;
using Log4NetMemoryOptimizedAppender;

namespace Log4NetRemoteAppenderClient
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var t1 = DateTime.Now;
            //for (int i = 0; i < 100000; i++)
            //{
            //    _log.Info(string.Format("{0} I Love u~", i));
            //}

            //StreamReader streamReader = new StreamReader(@"E:\WCF日志\20161021.log", Encoding.GetEncoding("GB2312"));
            StreamReader streamReader = new StreamReader(@"20161021.log", Encoding.GetEncoding("GB2312"));
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                _log.Info(line);
            }

            var t2 = DateTime.Now;
            
            Console.WriteLine(string.Format("测试完成，耗时{0} ms", (t2 - t1).TotalMilliseconds));
            Console.Read();

            MemoryCacheQueue.Instance().IsProcessExit = true;
            //MemoryCacheQueue.Instance().Flush();
        }

        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
