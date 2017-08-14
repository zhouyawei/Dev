using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ChatViaSocketServer
{
    class EchoAsyncServer : AsyncServer
    {
        public EchoAsyncServer(int numOfConnections)
            : base(numOfConnections)
        {
            
        }

        protected override void ProcessData(AsyncUserToken asyncUserToken, byte[] receivedBytes)
        {
            //base.ProcessData(asyncUserToken, receivedBytes);

            //var msg = Encoding.UTF8.GetString(receivedBytes);
            
            //Console.WriteLine(msg);
            base.SendData(asyncUserToken, receivedBytes);

            //_log.Debug(string.Format("EchoAsyncServer->ProcessData: msg = {0}", msg));
        }

        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
