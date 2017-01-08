using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatViaWCFClient
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        public LoginWindow(IChatFactory chatFactory, ILoginManager loginState, IRefresh refresh, Window parentWindow, string promptString)
        {
            this.DataContext = this;
            this.Owner = parentWindow;

            _chatFactory = chatFactory;
            _loginManager = loginState;
            _refresh = refresh;

            InitializeComponent();

            PromptString = promptString;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            DoLoginOrLogout();
        }

        private void _pwdTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoLoginOrLogout();
            }
        }

        private void DoLoginOrLogout()
        {
            var isLogin = PromptString == "登录" ? true : false;
            _loginManager.UserState = string.Format("{0}{1}", _userNameTextBox.Text, isLogin ? "在线" : "离线");
            _loginManager.UserId = _userNameTextBox.Text;
            _loginManager.Pwd = _pwdTextBox.Password;

            _loginManager.IsLogin = isLogin;

            this.Close();
        }

        private string _promptString = null;
        public string PromptString
        {
            get { return _promptString; }
            set { _promptString = value; NotifyPropertyChanged("PromptString"); }
        }

        private IChatFactory _chatFactory = null;
        private ILoginManager _loginManager = null;
        private IRefresh _refresh = null;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
