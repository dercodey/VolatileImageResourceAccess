using System;
using System.ServiceModel;
using System.Threading.Tasks;

using ServiceModelEx;

using PheonixRt.Mvvm.Services;
using NServiceBus;
using NServiceBus.Logging;
using Contracts.DicomLoader;


public class DicomLoaderManagerHelper : 
    IHandleMessages<ImageStored>,
    IHandleMessages<StructureStored>,
    IHandleMessages<AssociationClosed>,
    IHandleMessages<ScanCompleted>
{
    public static event Action<string, Guid, double> ImageStoredEvent = delegate { };
    public static event Action<string, Guid> StructureStoredEvent = delegate { };
    public static event Action<string, string[]> AssociationClosedEvent = delegate { };
    public static event Action<string> ScanCompletedEvent = delegate { };

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
    
    public Task Handle(ImageStored message, IMessageHandlerContext context)
    {
        // log.Info($"Handling: ImageStored for Image Id: {message.ImageGuid}");
        string methodID = string.Empty;
        ImageStoredEvent(methodID, message.ImageGuid, message.RepoGb);

        return Task.CompletedTask;
    }

    public Task Handle(StructureStored message, IMessageHandlerContext context)
    {
        // log.Info($"Handling: ImageStored for Image Id: {message.ImageGuid}");
        string methodID = string.Empty;
        StructureStoredEvent(methodID, message.StructureGuid);

        return Task.CompletedTask;
    }

    public Task Handle(AssociationClosed message, IMessageHandlerContext context)
    {
        System.Diagnostics.Trace.WriteLine(string.Format("AssociationClosed MsgId={0}", context.MessageId));

        // log.Info($"Handling: ImageStored for Image Id: {message.ImageGuid}");
        string methodID = string.Empty;
        AssociationClosedEvent(methodID, message.SeriesInstanceUids);

        return Task.CompletedTask;
    }

    public Task Handle(ScanCompleted message, IMessageHandlerContext context)
    {
        // log.Info($"Handling: ImageStored for Image Id: {message.ImageGuid}");
        string methodID = string.Empty;
        ScanCompletedEvent(methodID);

        return Task.CompletedTask;
    }
}