using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaWCFClient
{
    public interface ILoginState
    {
        string UserId { get; set; }
        string Pwd { get; set; }
        bool IsLogin { get; set; }
    }
}
