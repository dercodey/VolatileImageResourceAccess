using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.ResampleServiceContracts;

namespace PheonixRt.ResampleManagerService
{
    /// <summary>
    /// interface for calling in to the ResampleManager
    /// </summary>
    [ServiceContract]
    public interface IResampleManager
    {
        [OperationContract(IsOneWay = true)]
        void ResampleImageVolume(ImageVolumeResampleRequest request);
    }
}
