using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaWCFServer.Server
{
    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(string userId, string messageContent);

        [OperationContract(IsOneWay = true)]
        void Refresh();
    }
}
