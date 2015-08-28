using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using PheonixRt.DataContracts;
using PheonixRt.ResampleServiceContracts;

namespace PheonixRt.ResampleEngineService
{
    [ServiceContract]
    public interface IResampleEngine
    {
        /// <summary>
        /// resamples an image volume based on information in the request contract
        /// </summary>
        /// <param name="request">request contract describing the input </param>
        /// <returns></returns>
        [OperationContract]
        ImageVolumeResampleResponse ResampleImageVolume(ImageVolumeResampleRequest request);
    }
}
