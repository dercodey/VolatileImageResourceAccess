using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using ServiceModelEx;

using PheonixRt.DataContracts;

using PheonixRt.MeshingServiceContracts;

using PheonixRt.Mvvm.Services;
using PheonixRt.Mvvm.MeshingManagerServiceReference1;

[ServiceContract]
public interface IMeshingResponse
{
    [OperationContract(IsOneWay = true)]
    void OnMeshingDone(MeshingResponse response);
}

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
class MeshingManagerHelper : QueuedResponseBase, IMeshingResponse
{
    public static event Action<string, MeshingResponse> MeshCompleteEvent = delegate { };

    static string _responseQueueName = "MeshingResponseQueue";

    public static void StartResponseHost()
    {
        QueuedResponseBase.StartResponseHost<MeshingManagerHelper>(_responseQueueName);
    }

    public static void SetupResponseHeader(Guid methodId)
    {
        QueuedResponseBase.SetupResponseHeader(_responseQueueName, methodId);
    }

    public void OnMeshingDone(MeshingResponse response)
    {
        ResponseContext responseContext =
            OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                "ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        MeshCompleteEvent(methodID, response);
    }
}
