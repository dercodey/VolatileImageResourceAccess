using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using ServiceModelEx;

using PheonixRt.DataContracts;

using PheonixRt.Mvvm.Services;

[ServiceContract]
public interface IMprGenerationResponse
{
    [OperationContract(IsOneWay = true)]
    void OnMprDone(ImageDataContract idc, DateTime requestTime);
}

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
class MprGenerationHelper : QueuedResponseBase, IMprGenerationResponse
{
    public static event Action<string, ImageDataContract, DateTime> MprGenerationDoneEvent = delegate { };

    static string _responseQueueName = "MprGenerationResponseQueue";

    public static void StartResponseHost()
    {
        QueuedResponseBase.StartResponseHost<MprGenerationHelper>(_responseQueueName);
    }

    public static void SetupResponseHeader(Guid methodId)
    {
        QueuedResponseBase.SetupResponseHeader(_responseQueueName, methodId);
    }

    public void OnMprDone(ImageDataContract idc, DateTime requestTime)
    {
        ResponseContext responseContext =
            OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                "ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        MprGenerationDoneEvent(methodID, idc, requestTime);
    }
}