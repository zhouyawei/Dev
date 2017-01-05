using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ChatViaWCFServer.Server
{
    class ChatImpl : IChat
    {
        static ChatImpl()
        {
            _onlineUserHashtable = Hashtable.Synchronized(new Hashtable());
            _onlineUserChannelHashtable = Hashtable.Synchronized(new Hashtable());
        }

        public void Login(string userId, string pwd)
        {
            try
            {
                var callbackChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                if (!_onlineUserHashtable.ContainsKey(userId))
                {
                    if (callbackChannel != null)
                    {
                        _onlineUserHashtable.Add(userId, callbackChannel);
                        _log.Info(string.Format("用户{0}登录成功!", userId));
                    }
                }
                else
                {
                    _onlineUserHashtable[userId] = callbackChannel;
                    _log.Info(string.Format("用户{0}再次登录成功!", userId));
                }
                
                var communicationObject = callbackChannel as ICommunicationObject;
                communicationObject.Closed += communicationObject_Closed;

                if (!_onlineUserChannelHashtable.ContainsKey(callbackChannel))
                {
                    if (callbackChannel != null)
                    {
                        _onlineUserChannelHashtable.Add(callbackChannel, userId);
                        _log.Info(string.Format("用户{0}的通道建立成功!", userId));
                    }
                }
                else
                {
                    _onlineUserChannelHashtable[callbackChannel] = userId;
                    _log.Info(string.Format("用户{0}的通道再次建立成功!", userId));
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("ChatImpl->Login出现异常{0}", e);
            }
        }

        void communicationObject_Closed(object sender, EventArgs e)
        {
            var callbackChannel = sender;
            if (_onlineUserChannelHashtable.ContainsKey(callbackChannel))
            {
                var userId = _onlineUserChannelHashtable[callbackChannel];
                _log.Info(string.Format("用户{0}的通道已经关闭!", userId));
            }
        }

        public void Logout(string userId, string pwd)
        {
            try
            {
                if (_onlineUserHashtable.ContainsKey(userId))
                {
                    var callbackChannel = _onlineUserHashtable[userId];
                    _onlineUserHashtable.Remove(userId);
                    _onlineUserChannelHashtable.Remove(callbackChannel);
                    _log.Info(string.Format("用户{0}注销成功", userId));
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("ChatImpl->Logout出现异常{0}", e);
            }
        }

        public void SendMessage(string userId, string messageContet)
        {
            try
            {
                if (_onlineUserHashtable.ContainsKey(userId))
                {
                    var callbackChannel = _onlineUserHashtable[userId] as IChatCallback;
                    var fromChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                    var fromUserId = _onlineUserChannelHashtable[fromChannel] as string;
                    if (callbackChannel != null)
                    {
                        callbackChannel.ReceiveMessage(fromUserId, messageContet);
                    }
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("ChatImpl->SendMessage出现异常{0}", e);
            }
        }

        public IList<string> GetOnlineUserList()
        {
            IList<string> onlineUsers = new List<string>();
            try
            {
                foreach (var key in _onlineUserHashtable.Keys)
                {
                    var userId = key as string;
                    onlineUsers.Add(userId);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("ChatImpl->GetOnlineUserList出现异常{0}", e);
            }
            
            return onlineUsers;
        }

        private static Hashtable _onlineUserHashtable = null;
        private static Hashtable _onlineUserChannelHashtable = null;
        private static ILog _log = LogManager.GetLogger(typeof(ChatImpl));
    }
}
