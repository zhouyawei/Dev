using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketServer
{
    class BufferManager
    {
        public BufferManager(int totalBytesInBuffer, int bufferBytesForSingleSAEA)
        {
            _totalBytesInBuffer = totalBytesInBuffer;
            _bufferBytesForSingleSAEA = bufferBytesForSingleSAEA;
        }

        public void Init()
        {
            _buffer = new byte[_totalBytesInBuffer];
        }

        public bool SetBuffer(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (_freeIndexPool.Count > 0)
            {
                socketAsyncEventArgs.SetBuffer(_buffer, (int)_freeIndexPool.Pop(), _bufferBytesForSingleSAEA);
            }
            else
            {
                if ((_totalBytesInBuffer - _bufferBytesForSingleSAEA) < _currentIndex)
                {
                    return false;
                }
                
                socketAsyncEventArgs.SetBuffer(_buffer, _currentIndex, _bufferBytesForSingleSAEA);
                _currentIndex += _bufferBytesForSingleSAEA;
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            _freeIndexPool.Push(socketAsyncEventArgs.Offset);
            socketAsyncEventArgs.SetBuffer(null, 0, 0);
        }

        private byte[] _buffer = null;
        private int _totalBytesInBuffer;
        private int _currentIndex = 0;
        private int _bufferBytesForSingleSAEA;
        private Stack _freeIndexPool = Stack.Synchronized(new Stack());
    }
}
