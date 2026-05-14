using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common.Models;
using Common.Responses;

namespace Common.Contracts
{
    [ServiceContract]
    public interface IBatteryService
    {
        [OperationContract]
        AckResponse StartSession(EisMeta meta);

        [OperationContract]
        AckResponse PushSample(EisMeta sample);

        [OperationContract]
        AckResponse EndSession();
    }
}
