using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketServer
{
    class SocketAsyncEventArgsPool
    {
        public void Push(SocketAsyncEventArgs e)
        {
            _stack.Push(e);
        }

        public SocketAsyncEventArgs Pop()
        {
            return _stack.Pop() as SocketAsyncEventArgs;
        }

        private readonly Stack _stack = Stack.Synchronized(new Stack());
    }
}
