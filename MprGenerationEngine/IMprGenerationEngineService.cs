using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.DataContracts;
using MprGenerationContracts;

namespace MprGenerationEngine
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IMprGenerationEngineService
    {
        [OperationContract(IsOneWay=true)]
        void GenerateMpr(MprGenerationRequestV1 request);
    }
}
