using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaWCFClient
{
    public interface ILoginManager
    {
        string UserState { get; set; }
        string UserId { get; set; }
        string Pwd { get; set; }
        bool IsLogin { get; set; }
    }
}
