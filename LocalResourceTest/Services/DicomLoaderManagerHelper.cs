using System;
using System.ServiceModel;
using System.Threading.Tasks;

using ServiceModelEx;

using PheonixRt.Mvvm.Services;
using NServiceBus;
using NServiceBus.Logging;
using Contracts.DicomLoader;

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
public class DicomLoaderManagerHelper : 
    QueuedResponseBase, 
    IImageStoredResponse,
    IHandleMessages<ImageStored>
{
    public static event Action<string, Guid, double> ImageStoredEvent = delegate { };
    public static event Action<string, Guid> StructureStoredEvent = delegate { };
    public static event Action<string, string[]> AssociationClosedEvent = delegate { };
    public static event Action<string> ScanCompletedEvent = delegate { };

    static string _responseQueueName = "ImageStoredResponseQueue";

    static IEndpointInstance _endpointInstance = null;
    static ILog log = LogManager.GetLogger<DicomLoaderManagerHelper>();

    static DicomLoaderManagerHelper()
    {
        _endpointInstance = ConfigureSBEndpoint().GetAwaiter().GetResult();

        // Subscription happens automatically, via mapping in app.config
    }

    private static async Task<IEndpointInstance> ConfigureSBEndpoint()
    {
        var endpointConfiguration =
            new EndpointConfiguration("DicomLoaderManagerHelper");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.SendFailedMessagesTo("error");

        var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        return endpointInstance;
    }

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

        // ImageStoredEvent(methodID, imageGuid, repoGb);
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

    public Task Handle(ImageStored message, IMessageHandlerContext context)
    {
        // log.Info($"Handling: ImageStored for Image Id: {message.ImageGuid}");
        string methodID = string.Empty;
        ImageStoredEvent(methodID, message.ImageGuid, message.RepoGb);

        return Task.CompletedTask;
    }
}