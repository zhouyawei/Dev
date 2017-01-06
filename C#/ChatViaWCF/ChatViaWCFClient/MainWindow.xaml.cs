using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatViaWCFClient.WCFChat;
using log4net;

namespace ChatViaWCFClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, IChatCallback, IChatFactory, ILoginManager, IRefresh
    {
        public MainWindow()
        {
            Thread.CurrentThread.Name = "Main线程";

            InitializeComponent();

            Refresh();
        }

        public void Refresh()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    RefreshLoginState();
                });

                /*登录状态下通道开放，注销状态下通道关闭，不做刷新*/
                if (this.IsLogin)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        RefreshOnlineUsers();
                    });
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void RefreshLoginState()
        {
            _userIdTextBlock.Text = UserState;
        }

        private void RefreshOnlineUsers()
        {
            ChatClient chatClient = GetChatClient();
            _friendListBox.ItemsSource = chatClient.GetOnlineUserListBesidesMe();
        }

        public ChatClient GetChatClient()
        {
            if (_chatClient != null)
                return _chatClient;

            lock (_locker)
            {
                if (_chatClient == null)
                {
                    InstanceContext instanceContext = new InstanceContext(this);
                    _chatClient = new ChatClient(instanceContext);
                    var communicationObject = _chatClient as ICommunicationObject;
                    if (communicationObject != null)
                    {
                        communicationObject.Closing += communicationObject_Closing;
                        communicationObject.Closed += CommunicationObjectOnClosed;
                    }
                }
                return _chatClient;
            }
        }

        private void communicationObject_Closing(object sender, EventArgs e)
        {
            MessageBox.Show("即将关闭与服务器的连接");
        }

        private void CommunicationObjectOnClosed(object sender, EventArgs eventArgs)
        {
            MessageBox.Show("与服务器的连接已关闭");
        }

        public void ReceiveMessage(string userId, string messageContent)
        {
            try
            {
                _log.Debug(string.Format("Thread ID = {0}, Thread Name = {1}", Thread.CurrentThread.ManagedThreadId,
                    Thread.CurrentThread.Name));
                this.Dispatcher.Invoke(() =>
                {
                    _msgTextBox.Text += string.Format("{0} {1}: {2}\n", DateTime.Now.ToString("HH:mm:ss"), userId, messageContent);    
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void _loginMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(this, this, this, this, "登录");
            loginWindow.ShowDialog();
        }

        private void _logoutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(this, this, this, this, "注销");
            loginWindow.ShowDialog();
        }

        private void _sendMsgButton_OnClick(object sender, RoutedEventArgs e)
        {
            var msg = _sendMsgTextBox.Text;
            if (_friendListBox.SelectedIndex != -1 && !string.IsNullOrEmpty(msg))
            {
                var friendId = _friendListBox.SelectedItem as string;
                try
                {
                    GetChatClient().SendMessage(friendId, msg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public string UserId { get; set; }

        public string Pwd { get; set; }

        public string UserState { get; set; }

        private bool _isLogin = false;
        private bool? _isLoginLastState = null;
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                if (_isLoginLastState.HasValue && _isLoginLastState.Value == _isLogin)
                {
                    _log.Info(string.Format("_isLoginLastState = {0}, _isLogin = {1}相等，不再做登录或注销", _isLoginLastState, _isLogin));
                }
                else
                {
                    _isLogin = value;
                    if (_isLogin)
                    {
                        GetChatClient().Login(UserId, Pwd);
                    }
                    else
                    {
                        GetChatClient().Logout(UserId, Pwd);
                        GetChatClient().Close();
                    }
                    Refresh();
                    _isLoginLastState = _isLogin;
                }
            }
        }

        private ChatClient _chatClient = null;

        private static object _locker = new object();

        private static ILog _log = LogManager.GetLogger(typeof(MainWindow));
    }
}
