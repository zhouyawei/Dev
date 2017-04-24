using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketServer
{
    class UserTokenPool
    {
        public void Push(AsyncUserToken token)
        {
            _stack.Push(token);
        }

        public AsyncUserToken Pop()
        {
            return _stack.Pop() as AsyncUserToken;
        }

        private readonly Stack _stack = Stack.Synchronized(new Stack());
    }
}
