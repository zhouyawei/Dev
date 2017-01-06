using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ChatViaWCFClient
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        public void Initialize(Window mainWindow)
        {
            _chatFactory = mainWindow as IChatFactory;
            _loginManager = mainWindow as ILoginManager;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_chatFactory != null)
            {
                try
                {
                    _loginManager.IsLogin = false;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            base.OnExit(e);
        }

        private static IChatFactory _chatFactory = null;
        private static ILoginManager _loginManager = null;
    }
}
