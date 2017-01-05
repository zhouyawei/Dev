using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaWCFClient
{
    public class Program
    {
        /// <summary>
        /// 应用程序入口点
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void Main(string[] args)
        {
            App app = new App();
            var mainWindow = new MainWindow();
            app.Initialize(mainWindow);
            app.Run(mainWindow);
        }
    }
}
