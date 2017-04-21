﻿using System;
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
            _bufferManager = new BufferManager(receiveBufferSize*numOfConnections*OPS_TO_PREALLOC,
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
            _log.Debug("异步Accept调用");
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
                Interlocked.Add(ref _totalBytesReceived, (long) readEventArgs.BytesTransferred);
                _log.Info(string.Format("目前服务器已接受{0}字节的数据", _totalBytesReceived));

                /*读取缓冲区中的数据*/
                /*半包, 粘包*/
                try
                {
                    /*读取数据*/
                    byte[] dataTransfered = new byte[readEventArgs.BytesTransferred];
                    Array.Copy(readEventArgs.Buffer, readEventArgs.Offset, dataTransfered, 0, readEventArgs.BytesTransferred);
                    asyncUserToken.Buffer.AddRange(dataTransfered);

                    /* 4字节包头(长度)+包体*/
                    /* Header + Body */

                    /* 接收到的数据可能小于一个包的大小，需分多次接收
                     * 先判断包头的大小，够一个完整的包再处理
                     */

                    while (asyncUserToken.Buffer.Count > 4)
                    {
                        /*判断包的长度*/
                        byte[] lenBytes = asyncUserToken.Buffer.GetRange(0, 4).ToArray();
                        int bodyLen = BitConverter.ToInt32(lenBytes, 0);

                        var packageLength = 4 + bodyLen; //一个数据包的长度，4字节包头 + 包体的长度
                        var receivedLengthExcludeHeader = asyncUserToken.Buffer.Count - 4; //去掉包头之后接收的长度

                        /*接收的数据长度不够时，退出循环，让程序继续接收*/
                        if (bodyLen > receivedLengthExcludeHeader)
                        {
                            break;
                        }

                        /*接收的数据长度大于一个包的长度时，则提取出来，交给后面的程序去处理*/
                        byte[] receivedBytes = asyncUserToken.Buffer.GetRange(4, packageLength).ToArray();
                        asyncUserToken.Buffer.RemoveRange(0, packageLength); /*从缓冲区重移出取出的数据*/

                        /*抽象数据处理方法，receivedBytes是一个完整的包*/
                        ProcessData(asyncUserToken, receivedBytes);
                    }

                    /*继续接收, 非常关键的一步*/
                    if (!asyncUserToken.Socket.ReceiveAsync(readEventArgs))
                    {
                        this.ProcessReceive(readEventArgs);
                    }
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("AsyncServer->ProcessReceive出现异常, Exception = {0}", ex);
                }
            }
            else
            {
                CloseClientSocket(readEventArgs);
            }
        }

        /*抽象数据处理方法，receivedBytes是一个完整的包*/
        protected virtual void ProcessData(AsyncUserToken asyncUserToken, byte[] receivedBytes)
        {
            
        }

        /*对数据进行打包,然后再发送, dataInBytes是一个完整的数据包*/
        public void SendData(AsyncUserToken token, byte[] dataInBytes)
        {
            if (token == null || token.Socket == null || !token.Socket.Connected)
            {
                return;
            }

            try
            {
                /*对要发送的消息,制定简单协议,头4字节指定包的大小,方便客户端接收(协议可以自己定)*/
                byte[] buffer = new byte[dataInBytes.Length + 4];
                byte[] bodyLength = BitConverter.GetBytes(dataInBytes.Length); /*将body的长度转成字节数组*/

                Array.Copy(bodyLength, buffer, 4); //bodyLength
                Array.Copy(dataInBytes, 0, buffer, 4, dataInBytes.Length); //

                //token.Socket.Send(buff);  //这句也可以发送, 可根据自己的需要来选择  
                //新建异步发送对象, 发送消息  
                SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
                sendArg.UserToken = token;
                sendArg.SetBuffer(buffer, 0, buffer.Length);  //将数据放置进去.  

                token.Socket.SendAsync(sendArg);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServer->SendData出现异常, Exception = {0}", ex);
            }
        }  

        private void ProcessSend(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken asyncUserToken = socketAsyncEventArgs.UserToken as AsyncUserToken;
                // read the next block of data send from the client
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

        private const int BUFFER_SIZE = 4096;
        private const int DATA_CHUNK_LENGTH_HEADER = 4;

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
