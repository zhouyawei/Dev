﻿using System;
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
            _receiveBuffer = new MyList<byte>();
            _sendBuffer = new MyList<byte>();
            _receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _receiveSocketAsyncEventArgs.UserToken = this;
            _sendSocketAsyncEventArgs.UserToken = this;
        }

        public void Reset()
        {
            ReceiveBuffer.Clear();
        }

        public Socket Socket { get; set; }
        public MyList<byte> ReceiveBuffer { get { return _receiveBuffer; } }
        public MyList<byte> SendBuffer { get { return _sendBuffer; } }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get { return _receiveSocketAsyncEventArgs; } }
        public SocketAsyncEventArgs SendSocketAsyncEventArgs { get { return _sendSocketAsyncEventArgs; } }

        public AutoResetEvent SendAutoResetEvent = new AutoResetEvent(true);
        public AutoResetEvent ReceieveAutoResetEvent = new AutoResetEvent(true);
        
        private readonly MyList<byte> _receiveBuffer = null;
        private readonly MyList<byte> _sendBuffer = null;
        private readonly SocketAsyncEventArgs _receiveSocketAsyncEventArgs = null;
        private readonly SocketAsyncEventArgs _sendSocketAsyncEventArgs = null;
    }
}
