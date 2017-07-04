using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ChatViaSocketClient
{
    public abstract class AsyncClientV2Base
    {
        protected AsyncClientV2Base()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        public void Initialize()
        {
            _asyncUserToken = new AsyncUserToken();
            _asyncUserToken.ReceiveSocketAsyncEventArgs.SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
            _asyncUserToken.ReceiveSocketAsyncEventArgs.Completed += IO_Completed;
            _asyncUserToken.SendSocketAsyncEventArgs.SetBuffer(new byte[BUFFER_SIZE], 0, 0);
            _asyncUserToken.SendSocketAsyncEventArgs.Completed += IO_Completed;

            var remoteIP = IPAddress.Parse(_remoteServerIP);
            var remotePort = int.Parse(_remoteServerPort);
            EndPoint remotEndPoint = new IPEndPoint(remoteIP, remotePort);

            _asyncUserToken.ReceiveSocketAsyncEventArgs.RemoteEndPoint = remotEndPoint;
            _asyncUserToken.SendSocketAsyncEventArgs.RemoteEndPoint = remotEndPoint;
        }

        public void RunAsync()
        {
            _asyncUserToken.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            if (!_asyncUserToken.Socket.ConnectAsync(_asyncUserToken.SendSocketAsyncEventArgs))
            {
                ProcessConnect();
            }
        }

        virtual protected void DataReceived(byte[] receivedDataInBytes)
        {

        }

        protected void SendData(byte[] dataToSendInBytes)
        {
            /*
             * 通讯协议
             * 4字节Header + Body
             */

            int sendBufferSize = dataToSendInBytes.Length + DATA_CHUNK_LENGTH_HEADER;
            byte[] sendBuffer = new byte[sendBufferSize];

            /*计算4个字节的表示数据的长度*/
            byte[] dataTotalLength_InBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataToSendInBytes.Length));//数据总长度

            Array.Copy(dataTotalLength_InBytes, sendBuffer, dataTotalLength_InBytes.Length);
            Array.Copy(dataToSendInBytes, 0, sendBuffer, DATA_CHUNK_LENGTH_HEADER, dataToSendInBytes.Length);
            _asyncUserToken.SendBuffer.AddRange(sendBuffer);/*将发送的数据添加到发送缓冲区*/

            DoSend();
        }

        private void ProcessConnect()
        {
            /*if (!_clientSocket.SendAsync(_asyncUserToken.SendSocketAsyncEventArgs))
            {
                AfterSend();
            }*/

            /*ProcessSend();*/

            IsConnected = true;

            if (_asyncUserToken.Socket != null && !_asyncUserToken.Socket.ReceiveAsync(_asyncUserToken.ReceiveSocketAsyncEventArgs))
            {
                ProcessReceive();
            }
        }

        private void DoSend()
        {
            /*if (!_clientSocket.ReceiveAsync(_asyncUserToken.ReceiveSocketAsyncEventArgs))
            {
                AfterReceive();
            }*/

            while (_asyncUserToken.SendBuffer.Count > 0)
            {

                var size = Math.Min(_asyncUserToken.SendBuffer.Count, BUFFER_SIZE);
                byte[] dataToSend = _asyncUserToken.SendBuffer.GetRange(0, size).ToArray();
                _asyncUserToken.SendBuffer.RemoveRange(0, size);
                if (dataToSend.Length > 0)
                {
                    Array.Copy(dataToSend, _asyncUserToken.SendSocketAsyncEventArgs.Buffer, dataToSend.Length);
                    _asyncUserToken.SendAutoResetEvent.WaitOne();
                    _asyncUserToken.SendSocketAsyncEventArgs.SetBuffer(0, dataToSend.Length);
                    _asyncUserToken.SendAutoResetEvent.Set();
                    _asyncUserToken.SendAutoResetEvent.WaitOne();
                    if (_asyncUserToken.Socket != null && !_asyncUserToken.Socket.SendAsync(_asyncUserToken.SendSocketAsyncEventArgs))
                    {
                        _asyncUserToken.SendAutoResetEvent.Set();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessReceive()
        {
            /*if (!_clientSocket.SendAsync(_asyncUserToken.SendSocketAsyncEventArgs))
            {
                AfterSend();
            }*/

            if (_asyncUserToken.ReceiveSocketAsyncEventArgs.BytesTransferred != 0)
            {
                byte[] tempBuffer = new byte[_asyncUserToken.ReceiveSocketAsyncEventArgs.BytesTransferred];
                Array.Copy(_asyncUserToken.ReceiveSocketAsyncEventArgs.Buffer, tempBuffer, tempBuffer.Length);
                _asyncUserToken.ReceiveBuffer.AddRange(tempBuffer);

                while (_asyncUserToken.ReceiveBuffer.Count > DATA_CHUNK_LENGTH_HEADER)
                {
                    /*判断包的长度*/
                    byte[] lenBytes = _asyncUserToken.ReceiveBuffer.GetRange(0, DATA_CHUNK_LENGTH_HEADER).ToArray();
                    int bodyLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBytes, 0));//包体的长度

                    var packageLength = DATA_CHUNK_LENGTH_HEADER + bodyLen; //一个数据包的长度，4字节包头 + 包体的长度
                    var receivedLengthExcludeHeader = _asyncUserToken.ReceiveBuffer.Count - DATA_CHUNK_LENGTH_HEADER; //去掉包头之后接收的长度

                    /*接收的数据长度不够时，退出循环，让程序继续接收*/
                    if (receivedLengthExcludeHeader < bodyLen)
                    {
                        break;
                    }

                    /*接收的数据长度大于一个包的长度时，则提取出来，交给后面的程序去处理*/
                    byte[] receivedBytes = _asyncUserToken.ReceiveBuffer.GetRange(DATA_CHUNK_LENGTH_HEADER, bodyLen).ToArray();
                    _asyncUserToken.ReceiveBuffer.RemoveRange(0, packageLength); /*从缓冲区重移出取出的数据*/

                    /*抽象数据处理方法，receivedBytes是一个完整的包*/
                    DataReceived(receivedBytes);
                }

                /*继续接收, 非常关键的一步*/
                if (_asyncUserToken.Socket != null && !_asyncUserToken.Socket.ReceiveAsync(_asyncUserToken.ReceiveSocketAsyncEventArgs))
                {
                    ProcessReceive();
                }
            }
            else
            {
                CloseClientSocket();
            }
        }

        private void CloseClientSocket()
        {
            string clientIP = string.Empty;
            try
            {
                if (_asyncUserToken.Socket != null)
                {
                    clientIP = _asyncUserToken.Socket.RemoteEndPoint.ToString();
                    _asyncUserToken.Socket.Shutdown(SocketShutdown.Both);
                    _asyncUserToken.Socket.Close();
                    _asyncUserToken.Socket = null;
                    _asyncUserToken.ReceiveSocketAsyncEventArgs.AcceptSocket = null;
                    _asyncUserToken.SendSocketAsyncEventArgs.AcceptSocket = null;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServerBase->CloseClientSocket出现异常{0}, clientIP = {1}", ex, clientIP);
            }

            _asyncUserToken.Reset();

            IsConnected = false;
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect();
                    break;
                case SocketAsyncOperation.Receive:
                    _asyncUserToken.ReceieveAutoResetEvent.Set();
                    ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    _asyncUserToken.SendAutoResetEvent.Set();
                    //DoSend();
                    break;
            }
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                CloseClientSocket();
            }
        }

        public bool IsConnected = false;

        private string _remoteServerIP = ConfigurationManager.AppSettings["RemoteServerIP"];
        private string _remoteServerPort = ConfigurationManager.AppSettings["RemoteServerPort"];
        private AsyncUserToken _asyncUserToken = null;
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private object _sendLocker = new object();

        private const int BUFFER_SIZE = 4096;
        private const int DATA_CHUNK_LENGTH_HEADER = 4;
    }
}
