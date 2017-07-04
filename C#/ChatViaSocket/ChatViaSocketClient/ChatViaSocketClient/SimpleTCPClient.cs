using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ChatViaSocketClient
{
    public class SimpleTCPClient : AsyncClientV2Base
    {
        public SimpleTCPClient(string name)
        {
            _name = name;
        }

        public void SendTestData()
        {
            if (!IsConnected)
            {
                Thread.Sleep(10);
            }

            string content = GetSendData2();//AsyncClient.GetSendData();//
            byte[] messagesInBytes = Encoding.UTF8.GetBytes(content);
            base.SendData(messagesInBytes);
        }

        protected override void DataReceived(byte[] receivedDataInBytes)
        {
            var msg = Encoding.UTF8.GetString(receivedDataInBytes);

            Console.WriteLine(msg);
            //base.SendData(receivedDataInBytes);

            _log.Debug(string.Format("SimpleTCPClient->DataReceived: msg = {0}", msg));
        }

        private string GetSendData2()
        {
            //return _sendRecorder++.ToString();
            return "我是中国人!";
        }

        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string _name;
        private int _sendRecorder = 0;
    }
}
