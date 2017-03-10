using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetMemoryOptimizedAppender
{
    public static class InternalLogHelper
    {
        private static string _appLogPath = AppDomain.CurrentDomain.BaseDirectory + "Log4Net/";

        public static void WriteLog(string logContent)
        {
            //日志目录是否存在 不存在创建
            if (!Directory.Exists(_appLogPath))
            {
                Directory.CreateDirectory(_appLogPath);
            }

            File.AppendAllText(_appLogPath + DateTime.Now.ToString("yyyy-MM-dd") + ".log", logContent + "\r\n");
        }
    }
}
