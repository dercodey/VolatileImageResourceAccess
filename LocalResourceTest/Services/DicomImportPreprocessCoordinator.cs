using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ServiceModel;

using PheonixRt.DataContracts;
using PheonixRt.MeshingServiceContracts;
using PheonixRt.ResampleServiceContracts;

using PheonixRt.Mvvm.MeshingManagerServiceReference1;
using PheonixRt.Mvvm.ResampleManagerServiceReference1;
using PheonixRt.Mvvm.DicomScanServiceReference1;

using PheonixRt.Mvvm.Services;
using NServiceBus;

namespace PheonixRt.Mvvm
{
    /// <summary>
    /// top-level Coordinator that is responsible for:
    ///  * starting the DICOM scan
    ///  * responding to object completed events
    ///  * start post-processing activities
    /// </summary>
    public class DicomImportPreprocessCoordinator
    {
        static IEndpointInstance _endpointInstance = null;

        static DicomImportPreprocessCoordinator()
        {
            _endpointInstance = ConfigureSBEndpoint().GetAwaiter().GetResult();
        }

        private static async Task<IEndpointInstance> ConfigureSBEndpoint()
        {
            var endpointConfiguration = 
                new EndpointConfiguration("PheonixRt.Mvvm.DicomImportPreprocessCoordinator");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            return endpointInstance;
        }

        public DicomImportPreprocessCoordinator()
        {
            DicomLoaderManagerHelper.StructureStoredEvent += ImageStoredResponse_StructureStoredEvent;
            DicomLoaderManagerHelper.AssociationClosedEvent += ImageStoredResponse_AssociationClosedEvent;
        }

        /// <summary>
        /// starts the DICOM scan
        /// </summary>
        /// <param name="directory">top-level directory to be scanned</param>
        public static void StartScan(string directory)
        {
            var scanManager =
                new DicomScanManagerClient();
            using (OperationContextScope contextScope =
                new OperationContextScope(scanManager.InnerChannel))
            {
                var methodId = Guid.Empty; // Guid.NewGuid();

#if USE_ENDPOINT
                _endpointInstance.Send<ScanDirectory>();
#else
                scanManager.ScanDirectory(directory, false);
#endif
            }
            scanManager.Close();
        }

        /// <summary>
        /// queues up meshing for a structure
        /// </summary>
        /// <param name="structureGuid">guid of structure to be meshed</param>
        static void MeshStructure(Guid structureGuid)
        {
            var meshingManager = new MeshingManagerClient();
            using (OperationContextScope contextScope = 
                new OperationContextScope(meshingManager.InnerChannel))
            {
                var methodId = Guid.NewGuid();
                MeshingManagerHelper.SetupResponseHeader(methodId);

                // create the request object
                MeshingRequest request = new MeshingRequest();
                request.StructureGuid = structureGuid;

                // and queue the request
                meshingManager.MeshStructure(request);
            }
            meshingManager.Close();
        }

        void ImageStoredResponse_StructureStoredEvent(string methodID, Guid structureGuid)
        {
            MeshStructure(structureGuid);
        }

        /// <summary>
        /// performs resampling on a series
        /// </summary>
        /// <param name="seriesInstanceUID">the series UID to be used</param>
        static void ResampleVolumeForSeries(string seriesInstanceUID)
        {
            var resampleManager = new ResampleManagerClient();
            using (OperationContextScope contextScope = 
                new OperationContextScope(resampleManager.InnerChannel))
            {
                var methodId = Guid.NewGuid();
                ResampleDoneResponse.SetupResponseHeader(methodId);

                // create the request object
                ImageVolumeResampleRequest request = new ImageVolumeResampleRequest();
                request.SeriesInstanceUID = seriesInstanceUID;

                // and queue the request
                resampleManager.ResampleImageVolume(request);
            }
            resampleManager.Close();
        }

        void ImageStoredResponse_AssociationClosedEvent(string methodID, string[] seriesInstanceUIDs)
        {
            foreach (string seriesInstanceUID in seriesInstanceUIDs)
            {
                ResampleVolumeForSeries(seriesInstanceUID);
            }
        }
    }
}
