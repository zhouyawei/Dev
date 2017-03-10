using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using log4net.Config;
using Log4NetMemoryOptimizedAppender;

namespace Log4NetClientWindowsService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Thread.Sleep(20000);

            XmlConfigurator.Configure();

            StreamReader streamReader = new StreamReader(@"E:\WCF日志\20161021.log", true);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                _log.Info(line);
            }

            _log.Info("\"你还好吗?\"");

            InternalLogHelper.WriteLog("OnStart完成");
        }

        protected override void OnStop()
        {
            MemoryCacheQueue.Instance().IsProcessExit = true;
        }

        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
