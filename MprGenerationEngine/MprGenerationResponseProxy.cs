using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

using PheonixRt.DataContracts;


namespace MprGenerationEngine
{
    [ServiceContract]
    public interface IMprGenerationResponse
    {
        [OperationContract(IsOneWay = true)]
        void OnMprDone(ImageDataContract idc, DateTime requestTime);
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MprGenerationResponseProxy : ClientBase<IMprGenerationResponse>, IMprGenerationResponse
    {
        public MprGenerationResponseProxy()
        { }

        public MprGenerationResponseProxy(string endpointConfigurationName)
            : base(endpointConfigurationName)
        { }

        public MprGenerationResponseProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        { }

        public void OnMprDone(ImageDataContract idc, DateTime requestTime)
        {
            Channel.OnMprDone(idc, requestTime);
        }
    }
}
