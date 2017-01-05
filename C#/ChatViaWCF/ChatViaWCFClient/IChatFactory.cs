using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatViaWCFClient.WCFChat;

namespace ChatViaWCFClient
{
    public interface IChatFactory
    {
        ChatClient GetChatClient();
    }
}
