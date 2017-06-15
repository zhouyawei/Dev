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
    abstract class AsyncServer
    {
        public AsyncServer(int numOfConnections, int receiveBufferSize = BUFFER_SIZE)
        {
            _listenBacklog = _maxNumOfConnections = numOfConnections;
            _maxNumberOfConnectionsSemaphore = new Semaphore(_listenBacklog, _listenBacklog);
            _reveiveBufferSize = receiveBufferSize;
            _bufferManager = new BufferManager(receiveBufferSize * numOfConnections * OPS_TO_PREALLOC,
                _reveiveBufferSize);
            _userTokenPool = new UserTokenPool();
        }

        public void Init()
        {
            _bufferManager.Init();

            for (int i = 0; i < _listenBacklog; i++)
            {
                AsyncUserToken userToken = new AsyncUserToken();
                userToken.ReceiveSocketAsyncEventArgs.Completed += IO_Completed;
                userToken.ReceiveSocketAsyncEventArgs.UserToken = userToken;

                userToken.SendSocketAsyncEventArgs.Completed += IO_Completed;
                userToken.SendSocketAsyncEventArgs.UserToken = userToken;

                _bufferManager.SetBuffer(userToken.ReceiveSocketAsyncEventArgs);
                _bufferManager.SetBuffer(userToken.SendSocketAsyncEventArgs);

                _userTokenPool.Push(userToken);
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
            try
            {
                Interlocked.Increment(ref _numOfConnectedSocket);
                var userToken = _userTokenPool.Pop();
                userToken.Socket = acceptSocketAsyncEventArgs.AcceptSocket;
                userToken.ReceiveSocketAsyncEventArgs.AcceptSocket = acceptSocketAsyncEventArgs.AcceptSocket;
                userToken.SendSocketAsyncEventArgs.AcceptSocket = acceptSocketAsyncEventArgs.AcceptSocket;

                string clientIP = string.Empty;
                if (userToken.Socket != null)
                {
                    clientIP = userToken.Socket.RemoteEndPoint.ToString();
                }

                _log.Info(string.Format("客户端连接成功, 当前共有{0}个客户端连接到服务器, clientIP = {1}", _numOfConnectedSocket, clientIP));

                bool willRaiseEvent = userToken.Socket.ReceiveAsync(userToken.ReceiveSocketAsyncEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(userToken.ReceiveSocketAsyncEventArgs);
                }

                /*接收下一个请求*/
                StartAccept(acceptSocketAsyncEventArgs);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServerBase->ProcessAccept出现异常, Exception = {0}", ex);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs readEventArgs)
        {
            /*需要检查客户端是否关闭了连接*/
            AsyncUserToken asyncUserToken = readEventArgs.UserToken as AsyncUserToken;
            if (readEventArgs.BytesTransferred > 0 && readEventArgs.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref _totalBytesReceived, (long)readEventArgs.BytesTransferred);
                _log.Info(string.Format("目前服务器已接受{0}字节的数据", _totalBytesReceived));

                /*读取缓冲区中的数据*/
                /*半包, 粘包*/
                try
                {
                    /*读取数据*/
                    byte[] dataTransfered = new byte[readEventArgs.BytesTransferred];
                    Array.Copy(readEventArgs.Buffer, readEventArgs.Offset, dataTransfered, 0, readEventArgs.BytesTransferred);
                    asyncUserToken.Buffer.AddRange(dataTransfered);

                    /* 4字节包头(长度) + 包体*/
                    /* Header + Body */

                    /* 接收到的数据可能小于一个包的大小，需分多次接收
                     * 先判断包头的大小，够一个完整的包再处理
                     */

                    while (asyncUserToken.Buffer.Count > DATA_CHUNK_HEADER_LENGTH)
                    {
                        /*判断包的长度*/
                        byte[] lenBytes = asyncUserToken.Buffer.GetRange(0, DATA_CHUNK_HEADER_LENGTH).ToArray();
                        int bodyLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBytes, 0));//包体的长度

                        var packageLength = DATA_CHUNK_HEADER_LENGTH + bodyLen; //一个数据包的长度，4字节包头 + 包体的长度
                        var receivedLengthExcludeHeader = asyncUserToken.Buffer.Count - DATA_CHUNK_HEADER_LENGTH; //去掉包头之后接收的长度

                        /*接收的数据长度不够时，退出循环，让程序继续接收*/
                        if (receivedLengthExcludeHeader < bodyLen)
                        {
                            break;
                        }

                        /*接收的数据长度大于一个包的长度时，则提取出来，交给后面的程序去处理*/
                        byte[] receivedBytes = asyncUserToken.Buffer.GetRange(DATA_CHUNK_HEADER_LENGTH, bodyLen).ToArray();
                        asyncUserToken.Buffer.RemoveRange(0, packageLength); /*从缓冲区重移出取出的数据*/

                        /*抽象数据处理方法，receivedBytes是一个完整的包*/
                        ProcessData(asyncUserToken, receivedBytes);
                    }

                    /*继续接收, 非常关键的一步*/
                    if (asyncUserToken.Socket != null)
                    {
                        if (asyncUserToken.Socket.Connected)
                        {
                            if (!asyncUserToken.Socket.ReceiveAsync(readEventArgs))
                            {
                                this.ProcessReceive(readEventArgs);
                            }
                        }
                        else
                        {
                            string clientIP = string.Empty;
                            if (asyncUserToken.Socket != null)
                            {
                                clientIP = asyncUserToken.Socket.RemoteEndPoint.ToString();
                            }

                            _log.Debug(string.Format("AsyncServerBase->ProcessReceive: 已断开与远程客户端的连接, clientIP = {0}", clientIP));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("AsyncServer->ProcessReceive出现异常, Exception = {0}", ex);
                }
            }
            else
            {
                CloseClientSocket(asyncUserToken);
            }
        }

        /*抽象数据处理方法，receivedBytes是一个完整的包*/
        protected virtual void ProcessData(AsyncUserToken asyncUserToken, byte[] receivedBytes)
        {

        }

        /*对数据进行打包,然后再发送, dataInBytes是一个完整的数据包*/
        public void SendData(AsyncUserToken token, byte[] dataInBytes)
        {
            if (token == null)
            {
                _log.Info("AsyncServer->SendData: 发送未完成, token为空");
                return;
            }
            else if (token.Socket == null)
            {
                _log.Info("AsyncServer->SendData: 发送未完成, token.Socket为空");
                return;
            }
            else if (!token.Socket.Connected)
            {
                string clientIP = string.Empty;
                if (token.Socket != null)
                {
                    clientIP = token.Socket.RemoteEndPoint.ToString();
                }
                _log.Info(string.Format("AsyncServer->SendData: 发送未完成, token.Socket.Connected未连接到远程客户端, clientIP = {0}", clientIP));
                return;
            }

            try
            {
                /*对要发送的消息,制定简单协议,头4字节指定包的大小,方便客户端接收(协议可以自己定)*/
                //SendAsync(token, dataInBytes);
                //SendSync(token, dataInBytes);

                /*发的包长度过大，则分包发送*/
                byte[] buffer = new byte[dataInBytes.Length + DATA_CHUNK_HEADER_LENGTH];
                byte[] bodyLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataInBytes.Length)); /*将body的长度转成字节数组*/

                Array.Copy(bodyLength, buffer, 4); //bodyLength
                Array.Copy(dataInBytes, 0, buffer, 4, dataInBytes.Length); //将数据放置进去.  

                if (buffer.Length <= BUFFER_SIZE)
                {
                    token.Socket.Send(buffer);
                }
                else
                {
                    byte[] byteArrayTemp = buffer;
                    while (true)
                    {
                        byte[] dataToSend = byteArrayTemp.Take(BUFFER_SIZE).ToArray();
                        if (dataToSend.Length > 0)
                        {
                            token.Socket.Send(dataToSend);
                        }
                        else
                        {
                            break;
                        }
                        byteArrayTemp = byteArrayTemp.Skip(BUFFER_SIZE).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServer->SendData出现异常, Exception = {0}", ex);
            }
        }

        private void SendAsync(AsyncUserToken token, byte[] dataInBytes)
        {
            byte[] buffer = new byte[dataInBytes.Length + DATA_CHUNK_HEADER_LENGTH];
            byte[] bodyLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataInBytes.Length)); /*将body的长度转成字节数组*/

            Array.Copy(bodyLength, buffer, 4); //bodyLength
            Array.Copy(dataInBytes, 0, buffer, 4, dataInBytes.Length); //将数据放置进去.  
            Array.Copy(buffer, 0, token.SendSocketAsyncEventArgs.Buffer, token.SendSocketAsyncEventArgs.Offset, buffer.Length);

            token.IsSendSocketAsyncEventArgsCanBeUsedEvent.WaitOne();
            token.SendSocketAsyncEventArgs.SetBuffer(token.SendSocketAsyncEventArgs.Offset, dataInBytes.Length + DATA_CHUNK_HEADER_LENGTH);
            if (!token.Socket.SendAsync(token.SendSocketAsyncEventArgs))
            {
                ProcessSend(token.SendSocketAsyncEventArgs);
            }
        }

        private void SendSync(AsyncUserToken token, byte[] dataInBytes)
        {
            byte[] buffer = new byte[dataInBytes.Length + DATA_CHUNK_HEADER_LENGTH];
            byte[] bodyLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataInBytes.Length)); /*将body的长度转成字节数组*/

            Array.Copy(bodyLength, buffer, 4); //bodyLength
            Array.Copy(dataInBytes, 0, buffer, 4, dataInBytes.Length); //将数据放置进去.  
            token.Socket.Send(buffer);
        }

        private void ProcessSend(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            // done echoing data back to the client
            AsyncUserToken asyncUserToken = socketAsyncEventArgs.UserToken as AsyncUserToken;
            asyncUserToken.IsSendSocketAsyncEventArgsCanBeUsedEvent.Set();
            string clientIP = string.Empty;
            if (asyncUserToken.Socket != null)
            {
                clientIP = asyncUserToken.Socket.RemoteEndPoint.ToString();
            }

            var currentThreadID = Thread.CurrentThread.ManagedThreadId;

            if (socketAsyncEventArgs.SocketError == SocketError.Success)
            {
                _log.Debug(string.Format("AsyncServerBase->ProcessSend->发送成功, clientIP = {0}", clientIP));

                // read the next block of data send from the client
                //bool willRaiseEvent = asyncUserToken.Socket.ReceiveAsync(socketAsyncEventArgs);
                //if (!willRaiseEvent)
                //{
                //    ProcessReceive(socketAsyncEventArgs);
                //}
            }
            else
            {
                CloseClientSocket(asyncUserToken);
            }
        }

        private void CloseClientSocket(AsyncUserToken asyncUserToken)
        {
            string clientIP = string.Empty;
            try
            {
                if (asyncUserToken.Socket != null)
                {
                    clientIP = asyncUserToken.Socket.RemoteEndPoint.ToString();
                    asyncUserToken.Socket.Shutdown(SocketShutdown.Both);
                    asyncUserToken.Socket.Close();
                    asyncUserToken.Socket = null;
                    asyncUserToken.ReceiveSocketAsyncEventArgs.AcceptSocket = null;
                    asyncUserToken.SendSocketAsyncEventArgs.AcceptSocket = null;
                }

                asyncUserToken.Reset();

                Interlocked.Decrement(ref _numOfConnectedSocket);
                _maxNumberOfConnectionsSemaphore.Release();
                _log.Info(string.Format("客户端{0}关闭了一个连接, 当前的客户端连接数为{1}", clientIP, _numOfConnectedSocket));

                _userTokenPool.Push(asyncUserToken);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServerBase->CloseClientSocket出现异常{0}, clientIP = {1}", ex, clientIP);
            }
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
                    string clientIP = string.Empty;
                    if (e.AcceptSocket != null)
                    {
                        e.AcceptSocket.RemoteEndPoint.ToString();
                    }
                    _log.Warn(string.Format("The last operation completed on the socket was not a receive or send, clientIp = {0}", clientIP));
                    break;
            }
        }

        private const int BUFFER_SIZE = 4096;
        private const int DATA_CHUNK_HEADER_LENGTH = 4;

        private Socket _listenSocket = null;
        private int _listenBacklog = 1000;
        private Semaphore _maxNumberOfConnectionsSemaphore;
        private int _numOfConnectedSocket = 0;
        private int _maxNumOfConnections;
        private int _reveiveBufferSize;
        private long _totalBytesReceived = 0;
        private UserTokenPool _userTokenPool = null;
        private BufferManager _bufferManager;
        private const int OPS_TO_PREALLOC = 2;
        private ManualResetEvent _serverQuitEvent = new ManualResetEvent(false);
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
