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
        public LoginWindow(IChatFactory chatFactory, ILoginState loginState, IRefresh refresh, Window parentWindow, string promptString)
        {
            this.DataContext = this;
            this.Owner = parentWindow;

            _chatFactory = chatFactory;
            _loginState = loginState;
            _refresh = refresh;

            InitializeComponent();

            PromptString = promptString;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (PromptString == "登录")
            {
                _chatFactory.GetChatClient().Login(_userNameTextBox.Text, _pwdTextBox.Password);
                _loginState.IsLogin = true;
            }
            else
            {
                _chatFactory.GetChatClient().Logout(_userNameTextBox.Text, _pwdTextBox.Password);
                _loginState.IsLogin = false;
            }

            _loginState.UserId = string.Format("{0}{1}", _userNameTextBox.Text, _loginState.IsLogin ? "在线" : "离线");
            _loginState.Pwd = _pwdTextBox.Password;
            _refresh.Refresh();
            this.Close();
        }

        private string _promptString = null;
        public string PromptString
        {
            get { return _promptString; }
            set { _promptString = value; NotifyPropertyChanged("PromptString"); }
        }

        private IChatFactory _chatFactory = null;
        private ILoginState _loginState = null;
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
