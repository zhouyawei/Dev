﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    class AsyncUserToken
    {
        public AsyncUserToken()
        {
            _buffer = new List<byte>();
            _receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
        }

        public void Reset()
        {
            Buffer.Clear();
        }

        public Socket Socket { get; set; }
        public List<byte> Buffer { get { return _buffer; } }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs { get { return _receiveSocketAsyncEventArgs; } }
        public SocketAsyncEventArgs SendSocketAsyncEventArgs { get { return _sendSocketAsyncEventArgs; } }

        private readonly List<byte> _buffer = null;
        private readonly SocketAsyncEventArgs _receiveSocketAsyncEventArgs = null;
        private readonly SocketAsyncEventArgs _sendSocketAsyncEventArgs = null;
    }
}
