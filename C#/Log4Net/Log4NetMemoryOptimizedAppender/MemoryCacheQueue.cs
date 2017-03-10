using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using log4net.Util;
using Log4NetMemoryOptimizedAppender.Contract;
using Newtonsoft.Json;

namespace Log4NetMemoryOptimizedAppender
{
    public class MemoryCacheQueue
    {
        public static MemoryCacheQueue Instance()
        {
            if (_memoryCacheQueue == null)
            {
                _memoryCacheQueue = new MemoryCacheQueue();
            }

            return _memoryCacheQueue;
        }

        public void Initialize()
        {
            RestoreQueueFromFile();

            InternalLogHelper.WriteLog("MemoryCacheQueue->Initialize完成");
        }

        /*私有化构造函数*/
        private MemoryCacheQueue()
        {
            _safeQueue = Queue.Synchronized(new Queue());
            _workerThread = new Thread(DequeueWorkerFunc) { IsBackground = true, Name = "MemoryCacheQueueWorkerThread" };
            _workerThreadNeedToWorkAutoResetEvent = new AutoResetEvent(false);
        }

        ~MemoryCacheQueue()
        {

        }

        public void Enqueue(string data)
        {
            _safeQueue.Enqueue(data);
            _workerThreadNeedToWorkAutoResetEvent.Set();
        }

        public void StartDequeueWorkerThread()
        {
            _workerThread.Start();
            InternalLogHelper.WriteLog("MemoryCacheQueue->StartDequeueWorkerThread");
        }

        public void DoClean()
        {
            SaveQueueToFile();
        }

        public void Flush()
        {
            _workerThreadNeedToWorkAutoResetEvent.Set();
        }

        private void DequeueWorkerFunc()
        {
            while (true)
            {
                try
                {
                    if (!IsProcessExit)
                    {
                        _workerThreadNeedToWorkAutoResetEvent.WaitOne(500);

                        /*取队列中的数据发送到远程接口*/
                        DequeueWorkerFunc_SendToRemoteApi();
                    }
                    else
                    {
                        /*当IsProcessExit = true时该线程直接退出*/

                        break;
                    }
                }
                catch (Exception e)
                {
                    ErrorHandler.Error(string.Format("MemoryCacheQueue->DequeueWorkerFunc出现异常, Exception = {0}", e));
                    InternalLogHelper.WriteLog(e.ToString());
                }
            }
        }

        private void DequeueWorkerFunc_SendToRemoteApi()
        {
            while (_safeQueue.Count > 0 && !IsProcessExit)
            {
                var item = _safeQueue.Dequeue() as string;

                SendDataToRemoteApi(item);
            }
        }

        /*工作者线程调用的方法，只允许有一个工作者线程*/
        private void SendDataToRemoteApi(string data)
        {
            try
            {
                MakeSureRemoteDataSinkCanBeUsed();
                _remoteDataSink.NewLog(data);
            }
            catch (Exception e)
            {
                ErrorHandler.Error(string.Format("MemoryCacheQueue->SendDataToRemoteApi出现异常, Exception = {0}", e));
                InternalLogHelper.WriteLog(e.ToString());
            }
        }

        private void MakeSureRemoteDataSinkCanBeUsed()
        {
            if (_remoteDataSink == null)
            {
                CreateRemoteDataSink();
            }
            else
            {
                var communicationObject = _remoteDataSink as ICommunicationObject;
                if (communicationObject.State != CommunicationState.Opened &&
                    communicationObject.State != CommunicationState.Created &&
                    communicationObject.State != CommunicationState.Opening)
                {
                    CreateRemoteDataSink();
                }
            }
        }

        private void CreateRemoteDataSink()
        {
            try
            {
                Binding binding = new NetTcpBinding();
                EndpointAddress endpointAddress = new EndpointAddress(RemoteAddress);
                _remoteDataSink = ChannelFactory<IRemoteDataSink>.CreateChannel(binding, endpointAddress);
            }
            catch (Exception e)
            {
                ErrorHandler.Error(string.Format("MemoryCacheQueue->CreateRemoteDataSink出现异常, Exception = {0}", e));
                InternalLogHelper.WriteLog(e.ToString());
            }
        }

        private void SaveQueueToFile()
        {
            MemoryCacheQueuePersist memoryCacheQueuePersist = new MemoryCacheQueuePersist() { LastSaveTime = DateTime.Now };

            try
            {
                StreamWriter streamWriter = new StreamWriter(SAVEFILENAME, false, CurrentEncoding);

                var objArray = _safeQueue.ToArray();
                List<string> items = new List<string>();
                foreach (var o in objArray)
                {
                    items.Add(o as string);
                }
                memoryCacheQueuePersist.WorkingQueue = items;
                memoryCacheQueuePersist.IsAllSentToRemoteApi = _safeQueue.Count == 0;

                var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
                var serializeString = JsonConvert.SerializeObject(memoryCacheQueuePersist, settings);
                streamWriter.Write(serializeString);

                streamWriter.Close();
            }
            catch (Exception ex)
            {
                LogLog.Error(MethodBase.GetCurrentMethod().DeclaringType,
                    string.Format("SaveQueueToFile出现异常, ex = {0}", ex));
                InternalLogHelper.WriteLog(ex.ToString());
            }
        }

        private void RestoreQueueFromFile()
        {
            try
            {
                if (File.Exists(SAVEFILENAME))
                {
                    StreamReader streamReader = new StreamReader(SAVEFILENAME, true);

                    var fileContent = streamReader.ReadToEnd();
                    CurrentEncoding = streamReader.CurrentEncoding;

                    var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
                    var memoryCacheQueuePersist = JsonConvert.DeserializeObject<MemoryCacheQueuePersist>(fileContent, settings);

                    if (!memoryCacheQueuePersist.IsAllSentToRemoteApi)
                    {
                        this._safeQueue = Queue.Synchronized(new Queue(memoryCacheQueuePersist.WorkingQueue));
                    }

                    streamReader.Close();
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(MethodBase.GetCurrentMethod().DeclaringType,
                       string.Format("RestoreQueueFromFile出现异常, ex = {0}", ex));
                InternalLogHelper.WriteLog(ex.ToString());
            }
        }

        public string RemoteAddress
        {
            get
            {
                return _remoteAddress;
            }
            set
            {
                _remoteAddress = value;
            }
        }

        private Encoding CurrentEncoding
        {
            get
            {
                if (_currentEncoding == null)
                {
                    _currentEncoding = Encoding.Default;
                }
                return _currentEncoding;
            }
            set { _currentEncoding = value; }
        }

        public IErrorHandler ErrorHandler { get; set; }

        public volatile bool IsProcessExit = false;

        public class MemoryCacheQueuePersist
        {
            public bool IsAllSentToRemoteApi { get; set; }
            public List<string> WorkingQueue { get; set; }
            public DateTime LastSaveTime { get; set; }
        }

        private string _remoteAddress;
        private Queue _safeQueue = null;
        private Thread _workerThread = null;
        private AutoResetEvent _workerThreadNeedToWorkAutoResetEvent = null;
        private string _remoteApiAddress = null;
        private IRemoteDataSink _remoteDataSink;
        private static MemoryCacheQueue _memoryCacheQueue = null;
        private static readonly string SAVEFILENAME = string.Format("memorycachequeuefor{0}.dat", Assembly.GetEntryAssembly().GetName().Name);
        private Encoding _currentEncoding = null;
    }
}
