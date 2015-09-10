using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

using PheonixRt.DataContracts;


namespace ImageRenderManagerService
{
    [ServiceContract]
    public interface IImageRenderedResponse
    {
        [OperationContract(IsOneWay = true)]
        void OnImageRendered(ImageDataContract idc, DateTime requestTime);
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ImageRenderedResponseProxy : 
        ClientBase<IImageRenderedResponse>,
        IImageRenderedResponse
    {
        public ImageRenderedResponseProxy()
        { }

        public ImageRenderedResponseProxy(string endpointConfigurationName)
            : base(endpointConfigurationName)
        { }

        public ImageRenderedResponseProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        { }

        public void OnImageRendered(ImageDataContract idc, DateTime requestTime)
        {
            Channel.OnImageRendered(idc, requestTime);
        }
    }
}
