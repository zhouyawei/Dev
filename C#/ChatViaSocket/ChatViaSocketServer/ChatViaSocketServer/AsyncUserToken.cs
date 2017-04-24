using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketServer
{
    class AsyncUserToken
    {
        public AsyncUserToken()
        {
            ReceiveSocketAsyncEventArgs = new SocketAsyncEventArgs();

            SendSocketAsyncEventArgs = new SocketAsyncEventArgs();
        }

        public void Reset()
        {
            Buffer.Clear();
        }

        public Socket Socket { get; set; }
        public List<byte> Buffer { get; set; }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get; set; }
        public SocketAsyncEventArgs SendSocketAsyncEventArgs { get; set; }
    }
}
