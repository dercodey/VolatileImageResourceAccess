using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using ServiceModelEx;

using PheonixRt.MeshingServiceContracts;
using PheonixRt.MeshingManagerService.MeshingEngineServiceReference1;

namespace PheonixRt.MeshingManagerService
{
    /// <summary>
    /// handles queued requests to mesh structures
    /// </summary>
    public class MeshingManager : IMeshingManager
    {
        // the return address
        ResponseContext _responseContext;
        EndpointAddress _responseAddress;

        /// <summary>
        /// begin meshing for a particular structure
        /// </summary>
        /// <param name="request"></param>
        public void MeshStructure(MeshingRequest request)
        {
            // store the return address
            _responseContext =
                OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                    "ResponseContext", "ServiceModelEx");
            _responseAddress =
                new EndpointAddress(_responseContext.ResponseAddress);

            // notify that we are performing a meshing
            System.Diagnostics.Trace.WriteLine(
                string.Format("Meshing for structure {0} ({1} contours)",
                    request.StructureGuid,
                    request.ContourGuids != null 
                        ? request.ContourGuids.Count.ToString() : "?"));

            // create the engine
            MeshingEngineClient meshingEngine = new MeshingEngineClient();

            // synchronous call to perform meshing
            MeshingResponse response = meshingEngine.MeshStructure(request);
            meshingEngine.Close();

            // now construct the response proxy based on the return address
            MessageHeader<ResponseContext> responseHeader = new MessageHeader<ResponseContext>(_responseContext);
            NetMsmqBinding binding = new NetMsmqBinding("NoMsmqSecurity");
            MeshingResponseProxy proxy = new MeshingResponseProxy(binding, _responseAddress);

            // and set up the operation context to relay response info
            using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                // send the response
                proxy.OnMeshingDone(response);
            }

            proxy.Close();
        }
    }
}
