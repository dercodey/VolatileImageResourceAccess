using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using ServiceModelEx;

namespace PheonixRt.Mvvm.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class QueuedResponseBase
    {
        /// <summary>
        /// 
        /// </summary>
        public static void StartResponseHost<ResponseContractType>(string responseQueueName)
        {
            string queueNamePath =
                string.Format(".\\private$\\{0}", responseQueueName);

            try
            {
                // create the queue, if needed
                // see if it does not already exist
                if (MessageQueue.Exists(queueNamePath))
                {
                    // delete queue
                    MessageQueue.Delete(queueNamePath);
                }

                // create a transactional queue
                MessageQueue.Create(queueNamePath, true);

                string responseUrl = string.Format("http://localhost:{0}", _nextHttpPort);
                    //string.Format("net.msmq://localhost/private/{0}", responseQueueName);
                ServiceHost<ResponseContractType> host =
                    new ServiceHost<ResponseContractType>(responseUrl);
                host.Open();

                _nextHttpPort++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private static uint _nextHttpPort = 9000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodId"></param>
        public static void SetupResponseHeader(string responseQueueName, Guid methodId)
        {
            string responseQueueUri = string.Format("net.msmq://localhost/private/{0}",
                responseQueueName);
            var responseContext = new ResponseContext(responseQueueUri, methodId.ToString());
            var responseHeader = new MessageHeader<ResponseContext>(responseContext);
            OperationContext.Current.OutgoingMessageHeaders.Add(
                responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));
        }
    }
}
