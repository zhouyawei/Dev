using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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

namespace ChatViaWCFClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IChatCallback, IChatFactory, ILoginState, IRefresh
    {
        public MainWindow()
        {
            InitializeComponent();

            Refresh();
        }

        public void Refresh()
        {
            RefreshLoginState();
            RefreshOnlineUsers();
        }

        private void RefreshLoginState()
        {
            _userIdTextBlock.Text = UserId;
        }

        private void RefreshOnlineUsers()
        {
            ChatClient chatClient = GetChatClient();
            _friendListBox.ItemsSource = chatClient.GetOnlineUserListBesidesMe();
        }

        public ChatClient GetChatClient()
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

        public bool IsLogin { get; set; }

        private ChatClient _chatClient = null;

    }
}
