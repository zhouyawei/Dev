using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    public class AsyncUserToken
    {
        public AsyncUserToken()
        {
            _buffer = new MyList<byte>();
            _receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _receiveSocketAsyncEventArgs.UserToken = this;
            _sendSocketAsyncEventArgs.UserToken = this;
        }

        public void Reset()
        {
            Buffer.Clear();
        }

        public Socket Socket { get; set; }
        public MyList<byte> Buffer { get { return _buffer; } }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get { return _receiveSocketAsyncEventArgs; } }
        public SocketAsyncEventArgs SendSocketAsyncEventArgs { get { return _sendSocketAsyncEventArgs; } }
        
        private readonly MyList<byte> _buffer = null;
        private readonly SocketAsyncEventArgs _receiveSocketAsyncEventArgs = null;
        private readonly SocketAsyncEventArgs _sendSocketAsyncEventArgs = null;
    }
}
