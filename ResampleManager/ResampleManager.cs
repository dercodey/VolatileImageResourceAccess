using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using ServiceModelEx;

using PheonixRt.ResampleServiceContracts;
using PheonixRt.ResampleManagerService.ResampleEngineServiceReference1;

namespace PheonixRt.ResampleManagerService
{
    /// <summary>
    /// manages requests to the resample engine
    /// </summary>
    public class ResampleManager : IResampleManager
    {
        ResponseContext _responseContext;
        EndpointAddress _responseAddress;

        /// <summary>
        /// one-way call to resample an image volume
        /// </summary>
        /// <param name="seriesInstanceUID"></param>
        public void ResampleImageVolume(ImageVolumeResampleRequest request)
        {
            _responseContext =
                OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                    "ResponseContext", "ServiceModelEx");
            _responseAddress =
                new EndpointAddress(_responseContext.ResponseAddress);

            ResampleEngineClient re = new ResampleEngineClient();

            ImageVolumeResampleResponse response = re.ResampleImageVolume(request);
            System.Diagnostics.Trace.Assert(response.ResampledImageVolumeGuid.CompareTo(Guid.Empty) != 0);

            MessageHeader<ResponseContext> responseHeader = new MessageHeader<ResponseContext>(_responseContext);
            NetMsmqBinding binding = new NetMsmqBinding("NoMsmqSecurity");
            ResampleResponseProxy proxy = new ResampleResponseProxy(binding, _responseAddress);

            using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                proxy.OnResampleDone(response);
            }

            proxy.Close();
        }
    }
}
