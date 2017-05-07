using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    class MySocketAsyncEventArgs : SocketAsyncEventArgs
    {
        public bool IsUsing = false;
    }
}
