using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaWCFServer.Server
{
    [ServiceContract(Namespace = "http://eastmoney.com",
    CallbackContract = typeof(IChatCallback),
    SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract]
        void Login(string userId, string pwd);

        [OperationContract]
        void Logout(string userId, string pwd);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string userId, string messageContet);

        [OperationContract]
        IList<string> GetOnlineUserList();

        [OperationContract()]
        IList<string> GetOnlineUserListBesidesMe();
    }
}
