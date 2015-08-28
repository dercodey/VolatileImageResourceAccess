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
public interface IImageStoredResponse
{
    [OperationContract(IsOneWay = true)]
    void OnImageStored(Guid imageGuid, double repoGb);

    [OperationContract(IsOneWay = true)]
    void OnStructureStored(Guid structureGuid);

    [OperationContract(IsOneWay = true)]
    void OnAssociationClosed(string[] seriesInstanceUIDs);

    [OperationContract(IsOneWay = true)]
    void OnScanComplete();
}

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public class DicomLoaderManagerHelper : QueuedResponseBase, IImageStoredResponse
{
    public static event Action<string, Guid, double> ImageStoredEvent = delegate { };
    public static event Action<string, Guid> StructureStoredEvent = delegate { };
    public static event Action<string, string[]> AssociationClosedEvent = delegate { };
    public static event Action<string> ScanCompletedEvent = delegate { };

    static string _responseQueueName = "ImageStoredResponseQueue";

    public static void StartResponseHost()
    {
        QueuedResponseBase.StartResponseHost<DicomLoaderManagerHelper>(_responseQueueName);
    }

    public static void SetupResponseHeader(Guid methodId)
    {
        QueuedResponseBase.SetupResponseHeader(_responseQueueName, methodId);
    }

    public void OnImageStored(Guid imageGuid, double repoGb)
    {
        ResponseContext responseContext =
            OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                "ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        ImageStoredEvent(methodID, imageGuid, repoGb);
    }

    public void OnStructureStored(Guid structureGuid)
    {
        var operationContext = OperationContext.Current;
        ResponseContext responseContext =
            operationContext.IncomingMessageHeaders.GetHeader<ResponseContext>(
                "ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        StructureStoredEvent(methodID, structureGuid);
    }

    public void OnAssociationClosed(string[] seriesInstanceUIDs)
    {
        var operationContext = OperationContext.Current;
        ResponseContext responseContext =
            operationContext.IncomingMessageHeaders.GetHeader<ResponseContext>(
                "ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        AssociationClosedEvent(methodID, seriesInstanceUIDs);
    }

    public void OnScanComplete()
    {
        var operationContext = OperationContext.Current;
        ResponseContext responseContext =
            operationContext.IncomingMessageHeaders.GetHeader<ResponseContext>(
                "ResponseContext", "ServiceModelEx");
        string methodID = responseContext.MethodId;

        ScanCompletedEvent(methodID);
    }
}