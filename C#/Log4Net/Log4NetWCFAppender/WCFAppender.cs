using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Log4NetWCFAppender.Contract;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Log4NetWCFAppender
{
    public class WCFAppender : AppenderSkeleton
    {
        public WCFAppender()
        {

        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            CreateRemoteDataSink();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var communicationObject = _remoteDataSink as ICommunicationObject;
                if (communicationObject.State == CommunicationState.Opened ||
                    communicationObject.State == CommunicationState.Created ||
                    communicationObject.State == CommunicationState.Opening)
                {
                    _remoteDataSink.NewLogs(loggingEvent.RenderedMessage);
                }
                else
                {
                    CreateRemoteDataSink();
                    _remoteDataSink.NewLogs(loggingEvent.RenderedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void CreateRemoteDataSink()
        {
            Binding binding = new NetTcpBinding();
            EndpointAddress endpointAddress = new EndpointAddress(RemoteAddress);
            _remoteDataSink = ChannelFactory<IRemoteDataSink>.CreateChannel(binding, endpointAddress);
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

        private IRemoteDataSink _remoteDataSink;
        private string remoteAddress;
    }
}
