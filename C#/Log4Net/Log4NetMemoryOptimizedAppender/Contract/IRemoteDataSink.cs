using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetMemoryOptimizedAppender.Contract
{
    [ServiceContract]
    public interface IRemoteDataSink
    {
        [OperationContract(IsOneWay = true)]
        void NewLog(string message);
    }
}
