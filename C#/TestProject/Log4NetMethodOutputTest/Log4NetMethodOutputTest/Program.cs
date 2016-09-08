using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]
namespace Log4NetMethodOutputTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog log = LogManager.GetLogger(typeof(Program));
            log.Info("输出方法名测试");
            A a = new A();
            a.Show(log);
        }
    }

    class A
    {
        public void Show(ILog log)
        {
            log.Info("来自A的输出方法名测试");
        }
    }
}
