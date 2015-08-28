using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

using PheonixRt.MeshingServiceContracts;

namespace PheonixRt.MeshingManagerService
{

    [ServiceContract]
    public interface IMeshingResponse
    {
        [OperationContract(IsOneWay = true)]
        void OnMeshingDone(MeshingResponse response);
    }


    public partial class MeshingResponseProxy : ClientBase<IMeshingResponse>, IMeshingResponse
    {
        public MeshingResponseProxy()
        { }

        public MeshingResponseProxy(string endpointConfigurationName)
            : base(endpointConfigurationName)
        { }

        public MeshingResponseProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        { }

        public void OnMeshingDone(MeshingResponse response)
        {
            Channel.OnMeshingDone(response);
        }
    }
}
