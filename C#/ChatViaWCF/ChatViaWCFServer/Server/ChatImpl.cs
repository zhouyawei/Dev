using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
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
                var communicationObject = callbackChannel as ICommunicationObject;
                var currentChannel = callbackChannel as IContextChannel;
                if (!_onlineUserHashtable.ContainsKey(userId))
                {
                    if (callbackChannel != null)
                    {
                        _onlineUserHashtable.Add(userId, callbackChannel);
                        _log.Info(string.Format("用户{0}登录成功! 会话ID = {1}", userId, currentChannel.SessionId));
                    }
                }
                else
                {
                    _onlineUserHashtable[userId] = callbackChannel;
                    _log.Info(string.Format("用户{0}再次登录成功! 会话ID = {1}", userId, currentChannel.SessionId));
                }


                communicationObject.Closed += communicationObject_Closed;

                if (!_onlineUserChannelHashtable.ContainsKey(callbackChannel))
                {
                    if (callbackChannel != null)
                    {
                        _onlineUserChannelHashtable.Add(callbackChannel, userId);
                        _log.Info(string.Format("用户{0}的通道建立成功! HashCode = {1}", userId,
                            communicationObject.GetHashCode()));
                    }
                }
                else
                {
                    _onlineUserChannelHashtable[callbackChannel] = userId;
                    _log.Info(string.Format("用户{0}的通道再次建立成功! HashCode = {1}",
                        userId, communicationObject.GetHashCode()));
                }

                ThreadPool.QueueUserWorkItem(RefreshAllClients);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("ChatImpl->Login出现异常{0}", e);
            }
        }

        private void RefreshAllClients(object state)
        {
            foreach (IChatCallback channel in _onlineUserChannelHashtable.Keys)
            {
                try
                {
                    channel.Refresh();
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("ChatImpl->RefreshAllClients出现异常{0}", e);
                }
            }
        }

        private void communicationObject_Closed(object sender, EventArgs e)
        {
            var callbackChannel = sender;
            var currentChannel = callbackChannel as IContextChannel;
            if (_onlineUserChannelHashtable.ContainsKey(callbackChannel))
            {
                var userId = _onlineUserChannelHashtable[callbackChannel] as string;
                _log.Info(string.Format("用户{0}的通道已经关闭! 会话ID = {1}, HashCode = {2}", userId,
                    currentChannel.SessionId, callbackChannel.GetHashCode()));

                /*移除关闭的通道*/
                RemoveChannelFromServer(userId, callbackChannel as IChatCallback);
            }
        }

        public void Logout(string userId, string pwd)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    if (_onlineUserHashtable.ContainsKey(userId))
                    {
                        try
                        {
                            var callbackChannel = _onlineUserHashtable[userId] as IChatCallback;
                            var currentChannel = callbackChannel as IContextChannel;
                            RemoveChannelFromServer(userId, callbackChannel);
                            _log.Info(string.Format("用户{0}注销成功, 会话ID = {1}, HashCode = {2}", userId,
                                currentChannel.SessionId, callbackChannel.GetHashCode()));
                        }
                        catch (Exception ex)
                        {
                            _log.ErrorFormat("ChatImpl->Logout移除通道出现异常{0}", ex);
                        }

                        ThreadPool.QueueUserWorkItem((x) =>
                        {
                            WriteOnlineUserHashtable();
                        });
                    }
                    else
                    {
                        _log.Info(string.Format("_onlineUserHashtable中没有userId = {0}的用户", userId));
                    }
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("ChatImpl->Logout出现异常{0}", e);
            }
        }

        private void RemoveChannelFromServer(string userId, IChatCallback callbackChannel)
        {
            _onlineUserHashtable.Remove(userId);
            _onlineUserChannelHashtable.Remove(callbackChannel);
        }

        private void WriteOnlineUserHashtable()
        {
            if (_onlineUserHashtable.Keys.Count > 0)
            {
                foreach (var key in _onlineUserHashtable.Keys)
                {
                    var userId = key as string;
                    _log.Debug(string.Format("当前在线用户: {0}", userId));
                }
            }
            else
            {
                _log.Debug("当前在线用户数为0");
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

        public IList<string> GetOnlineUserListBesidesMe()
        {
            var fromChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            IList<string> onlineUsers = new List<string>();
            try
            {
                foreach (DictionaryEntry dictionaryEntry in _onlineUserHashtable)
                {
                    var userId = dictionaryEntry.Key as string;
                    var otherChannel = dictionaryEntry.Value;
                    if (fromChannel != otherChannel)
                    {
                        onlineUsers.Add(userId);
                    }
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
