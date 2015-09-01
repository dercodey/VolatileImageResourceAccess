using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel.Activation;

using LocalResourceManager;
using DicomLoaderManager;

using PheonixRt.ResampleEngineService;
using PheonixRt.ResampleManagerService;

using PheonixRt.MeshingEngineService;
using PheonixRt.MeshingManagerService;

using MprGenerationEngine;

namespace LocalResourceTestServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupQueues();

            StartHost<LocalImageResourceManager>();
            StartHost<LocalGeometryResourceManager>();
            StartHost<DicomScanManager>();
            StartHost<ResampleEngine>();
            StartHost<ResampleManager>();
            StartHost<MeshingEngine>();
            StartHost<MeshingManager>();
            StartHost<MprGenerationEngineService>();

            foreach (var host in _hosts)
            {
                WriteHostInfo(host);
            }
            Console.ReadLine();
        }

        static void WriteHostInfo(ServiceHost host)
        {
            System.Console.WriteLine(String.Format("Host: {0}", 
                host.Description.Name));
            foreach (var endpoint in host.Description.Endpoints)
            {
                System.Console.WriteLine(String.Format("\tEndpoint: {0}\n\tContract: {1}", 
                    endpoint.Address, 
                    endpoint.Contract.ContractType.ToString()));
            }
        }

        /// <summary>
        /// MAKE SURE queues are set up before starting hosts
        /// </summary>
        public static void SetupQueues()
        {
            // create the queues (will do nothing if they already exist)
            foreach (var queueName in _queueNames)
            {
                // see if it does not already exist
                if (MessageQueue.Exists(queueName))
                {
                    // delete queue
                    MessageQueue.Delete(queueName);
                }

                // create a transactional queue
                MessageQueue.Create(queueName, true);
            }
        }

        static string[] _queueNames = new string[] 
        {
            @".\private$\dicomscanqueue",
            //@".\private$\imagestoredresponsequeue", 
            @".\private$\meshingqueue",
            //@".\private$\meshingresponsequeue",
            @".\private$\resamplerequestqueue",
            //@".\private$\resampleresponsequeue",
            @".\private$\mprgenerationrequestqueue",
            //@".\private$\mprgenerationresponsequeue",
        };

        static List<ServiceHost> _hosts = new List<ServiceHost>();
        static ServiceHostFactory _factory = new ServiceHostFactory();

        public static void StartHost<ServiceType>()
        {            
            ServiceHost host = new ServiceHost(typeof(ServiceType));
            host.Open();
            System.Diagnostics.Trace.Assert(host.State == CommunicationState.Opened);
            _hosts.Add(host);
        }
    }
}
