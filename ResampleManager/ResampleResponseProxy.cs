using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

using PheonixRt.DataContracts;
using PheonixRt.ResampleServiceContracts;

using PheonixRt.ResampleManagerService.ResampleEngineServiceReference1;

namespace PheonixRt.ResampleManagerService
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface IResampleResponse
    {
        [OperationContract(IsOneWay = true)]
        void OnResampleDone(ImageVolumeResampleResponse response);
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ResampleResponseProxy : ClientBase<IResampleResponse>, IResampleResponse
    {
        public ResampleResponseProxy()
        { }

        public ResampleResponseProxy(string endpointConfigurationName)
            : base(endpointConfigurationName)
        { }

        public ResampleResponseProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        { }

        public void OnResampleDone(ImageVolumeResampleResponse response)
        {
 	        Channel.OnResampleDone(response);
        }
    }
}
