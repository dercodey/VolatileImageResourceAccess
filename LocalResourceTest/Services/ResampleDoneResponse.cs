using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using PheonixRt.DataContracts;
using PheonixRt.ResampleServiceContracts;

using PheonixRt.Mvvm.Services;

using ServiceModelEx;


[ServiceContract]
public interface IResampleResponse
{
    [OperationContract(IsOneWay = true)]
    void OnResampleDone(ImageVolumeResampleResponse response);
}

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
class ResampleDoneResponse : QueuedResponseBase, IResampleResponse
{
    public static event Action<string, ImageVolumeResampleResponse> ResampleDoneEvent = delegate { };

    static string _responseQueueName = "ResampleResponseQueue";

    public static void StartResponseHost()
    {
        QueuedResponseBase.StartResponseHost<ResampleDoneResponse>(_responseQueueName);
    }

    public static void SetupResponseHeader(Guid methodId)
    {
        QueuedResponseBase.SetupResponseHeader(_responseQueueName, methodId);
    }

    public void OnResampleDone(ImageVolumeResampleResponse response)
    {
        ResponseContext responseContext =
            OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>("ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        ResampleDoneEvent(methodID, response);
    }
}