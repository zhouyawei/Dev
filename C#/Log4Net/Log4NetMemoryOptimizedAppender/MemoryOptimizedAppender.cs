using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace Log4NetMemoryOptimizedAppender
{
    public class MemoryOptimizedAppender : AppenderSkeleton
    {
        public override void ActivateOptions()
        {
            InternalLogHelper.WriteLog("MemoryOptimizedAppender->ActivateOptions");

            MemoryCacheQueue.Instance().RemoteAddress = RemoteAddress;
            MemoryCacheQueue.Instance().ErrorHandler = ErrorHandler;
            MemoryCacheQueue.Instance().Initialize();
            
            base.ActivateOptions();
            MemoryCacheQueue.Instance().StartDequeueWorkerThread(); /*Queue初始化完成后开始工作者线程*/
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            MemoryCacheQueue.Instance().Enqueue(loggingEvent.RenderedMessage);
        }

        protected override void OnClose()
        {
            try
            {
                base.OnClose();
                MemoryCacheQueue.Instance().DoClean();
            }
            catch (Exception ex)
            {
                LogLog.Error(typeof(MemoryOptimizedAppender), ex.ToString());
                InternalLogHelper.WriteLog(ex.ToString());
            }
        }

        public override bool Flush(int millisecondsTimeout)
        {
            MemoryCacheQueue.Instance().Flush();
            return base.Flush(millisecondsTimeout);
        }

        public string RemoteAddress
        {
            get
            {
                return remoteAddress;
            }
            set
            {
                remoteAddress = value;
            }
        }

        private string remoteAddress;
    }
}
