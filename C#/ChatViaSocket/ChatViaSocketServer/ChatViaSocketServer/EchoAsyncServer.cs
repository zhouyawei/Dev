using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            base.ProcessData(asyncUserToken, receivedBytes);

            var msg = Encoding.UTF8.GetString(receivedBytes);
            _stringBuilder.Append(msg);

            Console.Write(msg);
        }

        StringBuilder _stringBuilder = new StringBuilder();
    }
}
