using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

using ServiceModelEx;

using PheonixRt.DataContracts;

namespace DicomLoaderManager
{
    /// <summary>
    /// Return service contract for the DICOM scan.
    /// This may be better as a pub/sub interface
    /// </summary>
    [ServiceContract]
    public interface IImageStoredResponse
    {
        /// <summary>
        /// Indicates an image has been stored
        /// </summary>
        /// <param name="imageGuid">the ID of the stored image in local storage</param>
        [OperationContract(IsOneWay=true)]
        void OnImageStored(Guid imageGuid, double repoGb);

        /// <summary>
        /// Indicates that a structure has been stored
        /// </summary>
        /// <param name="structureGuid">the ID of the stored structure in local storage</param>
        [OperationContract(IsOneWay = true)]
        void OnStructureStored(Guid structureGuid);

        /// <summary>
        /// Indicates that an "association" has been completed (in this case, a single directory
        /// scan has completed)
        /// </summary>
        /// <param name="seriesInstanceUIDs">list of series instance UIDs that were found</param>
        [OperationContract(IsOneWay = true)]
        void OnAssociationClosed(string[] seriesInstanceUIDs);

        /// <summary>
        /// indicates completion of the scan
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnScanComplete();
    }


    public partial class ImageResponseClient : ClientBase<IImageStoredResponse>, IImageStoredResponse
    {
        // call in an operation context to capture response info
        public static void PrepareResponse()
        {
            _responseContext =
                OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                    "ResponseContext", "ServiceModelEx");
            _responseAddress =
                new EndpointAddress(_responseContext.ResponseAddress);

        }

        public static ImageResponseClient CreateProxy()
        {
            var responseHeader = new MessageHeader<ResponseContext>(_responseContext);
            var binding = new NetMsmqBinding("NoMsmqSecurity");
            var proxy = new ImageResponseClient(binding, _responseAddress);
            proxy._responseHeader = responseHeader;
            return proxy;
        }

        static ResponseContext _responseContext;
        static EndpointAddress _responseAddress;

        MessageHeader<ResponseContext> _responseHeader;

        public ImageResponseClient()
        { }

        public ImageResponseClient(string endpointConfigurationName)
            : base(endpointConfigurationName)
        { }

        public ImageResponseClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        { }

        public void OnImageStored(Guid imageGuid, double repoGb)
        {
            using (OperationContextScope scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    _responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                Channel.OnImageStored(imageGuid, repoGb);
            }
        }

        public void OnStructureStored(Guid structureGuid)
        {
            using (OperationContextScope scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    _responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                Channel.OnStructureStored(structureGuid);
            }
        }

        public void OnAssociationClosed(string[] seriesInstanceUIDs)
        {
            using (OperationContextScope scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    _responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                Channel.OnAssociationClosed(seriesInstanceUIDs);
            }
        }

        public void OnScanComplete()
        {
            using (OperationContextScope scope = new OperationContextScope(InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    _responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                Channel.OnScanComplete();
            }
        }

    }
}
