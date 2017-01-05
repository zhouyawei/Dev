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
            _loginState = mainWindow as ILoginState;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_chatFactory != null)
            {
                try
                {
                    _chatFactory.GetChatClient().Logout(_loginState.UserId, _loginState.Pwd);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            base.OnExit(e);
        }

        private static IChatFactory _chatFactory = null;
        private static ILoginState _loginState = null;
    }
}
