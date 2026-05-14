using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common.Models;
using Common.Responses;
using Common.Faults;


namespace Common.Contracts
{
    [ServiceContract]
    public interface IBatteryService
    {

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        AckResponse StartSession(EisMeta meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        AckResponse PushSample(EisSample sample);

        [OperationContract]
        AckResponse EndSession();
    }
}
