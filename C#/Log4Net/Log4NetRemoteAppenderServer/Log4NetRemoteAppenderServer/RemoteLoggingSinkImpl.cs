using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp; 
using log4net.Appender;
using log4net.Core;

namespace Log4NetRemoteAppenderServer
{
    public class RemoteLoggingSinkImpl : MarshalByRefObject, RemotingAppender.IRemoteLoggingSink
    {
        /*注册通道*/
        public void RegisterRemotingServerChannel()
        {
            if (_tcpChannel == null)
            {
                _tcpChannel = new TcpChannel(8085);

                // Setup remoting server 
                try
                {
                    ChannelServices.RegisterChannel(_tcpChannel, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                // Marshal the sink object 
                RemotingServices.Marshal(RemoteLoggingSinkImpl.Instance, "LoggingSink", typeof(RemotingAppender.IRemoteLoggingSink));
            } 
        }

        /*这里接收客户端传来的消息*/
        public void LogEvents(LoggingEvent[] events)
        {
            foreach (var loggingEvent in events)
            {
                Console.WriteLine(loggingEvent.RenderedMessage);
            }
        }

        public override object InitializeLifetimeService()
        {
            return base.InitializeLifetimeService();
        }


        /*私有化构造函数*/
        private RemoteLoggingSinkImpl()
        {
        }

        public static readonly RemoteLoggingSinkImpl Instance = new RemoteLoggingSinkImpl();

        private TcpChannel _tcpChannel;
    }
}
