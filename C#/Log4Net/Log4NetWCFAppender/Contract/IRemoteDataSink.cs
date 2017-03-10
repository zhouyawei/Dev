using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetWCFAppender.Contract
{
    [ServiceContract]
    public interface IRemoteDataSink
    {
        [OperationContract]
        void NewLogs(string message);
    }
}
