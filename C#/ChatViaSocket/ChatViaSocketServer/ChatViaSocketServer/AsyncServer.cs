using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ChatViaSocketServer
{
    class AsyncServer
    {
        public AsyncServer(int numOfConnections, int receiveBufferSize)
        {
            _listenBacklog = _maxNumOfConnections = numOfConnections;
            _maxNumberOfConnectionsSemaphore = new Semaphore(_listenBacklog, _listenBacklog);
            _reveiveBufferSize = receiveBufferSize;
            _bufferManager = new BufferManager(receiveBufferSize * numOfConnections * OPS_TO_PREALLOC,
                _reveiveBufferSize);
            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
        }

        public void Init()
        {
            _bufferManager.Init();

            for (int i = 0; i < _listenBacklog; i++)
            {
                SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
                socketAsyncEventArgs.Completed += IO_Completed;
                socketAsyncEventArgs.UserToken = new AsyncUserToken();

                _bufferManager.SetBuffer(socketAsyncEventArgs);

                _socketAsyncEventArgsPool.Push(socketAsyncEventArgs);
            }

        }

        public void Start(IPEndPoint ipEndPoint)
        {
            /*开始监听*/
            _listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(ipEndPoint);
            _listenSocket.Listen(_listenBacklog);

            StartAccept(null);

            _log.Info("服务启动成功");
            _serverQuitEvent.WaitOne();
            _log.Info("服务即将退出");
        }

        private void StartAccept(SocketAsyncEventArgs acceptSocketAsyncEventArgs)
        {
            if (acceptSocketAsyncEventArgs == null)
            {
                acceptSocketAsyncEventArgs = new SocketAsyncEventArgs();
                acceptSocketAsyncEventArgs.Completed += AcceptSocketAsyncEventArgs_Completed;
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptSocketAsyncEventArgs.AcceptSocket = null;
            }

            _maxNumberOfConnectionsSemaphore.WaitOne();

            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptSocketAsyncEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptSocketAsyncEventArgs);
            }
        }

        private void AcceptSocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            _log.Debug("同步Accept调用");
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptSocketAsyncEventArgs)
        {
            Interlocked.Increment(ref _numOfConnectedSocket);
            _log.Info(string.Format("客户端连接成功, 当前共有{0}个客户端连接到服务器", _numOfConnectedSocket));

            var readEventArgs = _socketAsyncEventArgsPool.Pop();
            var userToken = readEventArgs.UserToken as AsyncUserToken;
            userToken.Socket = acceptSocketAsyncEventArgs.AcceptSocket;

            bool willRaiseEvent = userToken.Socket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            /*接收下一个请求*/
            StartAccept(acceptSocketAsyncEventArgs);
        }

        private void ProcessReceive(SocketAsyncEventArgs readEventArgs)
        {
            /*需要检查客户端是否关闭了连接*/
            AsyncUserToken asyncUserToken = readEventArgs.UserToken as AsyncUserToken;
            if (readEventArgs.BytesTransferred > 0 && readEventArgs.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref _totalBytesReceived, (long)readEventArgs.BytesTransferred);
                _log.Info(string.Format("目前服务器已接受{0}字节的数据", _totalBytesReceived));

                /*将数据发回客户端*/
                readEventArgs.SetBuffer(readEventArgs.Offset, readEventArgs.BytesTransferred);
                bool willRaiseEvent = asyncUserToken.Socket.SendAsync(readEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessSend(readEventArgs);
                }
            }
            else
            {
                CloseClientSocket(readEventArgs);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs.SocketError == SocketError.Success)
            {
                AsyncUserToken asyncUserToken = socketAsyncEventArgs.UserToken as AsyncUserToken;
                bool willRaiseEvent = asyncUserToken.Socket.ReceiveAsync(socketAsyncEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(socketAsyncEventArgs);
                }
            }
            else
            {
                CloseClientSocket(socketAsyncEventArgs);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            AsyncUserToken asyncUserToken = socketAsyncEventArgs.UserToken as AsyncUserToken;

            try
            {
                asyncUserToken.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServer->CloseClientSocket出现异常{0}", ex);
            }

            asyncUserToken.Socket.Close();

            Interlocked.Decrement(ref _numOfConnectedSocket);
            _maxNumberOfConnectionsSemaphore.Release();
            _log.Info(string.Format("客户端关闭了一个连接, 当前的客户端连接数为{0}", _numOfConnectedSocket));

            _socketAsyncEventArgsPool.Push(socketAsyncEventArgs);
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private Socket _listenSocket = null;
        private int _listenBacklog = 1000;
        private Semaphore _maxNumberOfConnectionsSemaphore;
        private int _numOfConnectedSocket = 0;
        private int _maxNumOfConnections;
        private int _reveiveBufferSize;
        private long _totalBytesReceived = 0;
        private SocketAsyncEventArgsPool _socketAsyncEventArgsPool = null;
        private BufferManager _bufferManager;
        private const int OPS_TO_PREALLOC = 2;
        private ManualResetEvent _serverQuitEvent = new ManualResetEvent(false);
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
