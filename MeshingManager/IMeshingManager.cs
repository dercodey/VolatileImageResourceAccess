using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.MeshingServiceContracts;

namespace PheonixRt.MeshingManagerService
{
    /// <summary>
    /// service interface for queued requests to mesh structures
    /// </summary>
    [ServiceContract]
    public interface IMeshingManager
    {
        /// <summary>
        /// handle single queued request
        /// </summary>
        /// <param name="request">request for meshing</param>
        [OperationContract(IsOneWay=true)]
        void MeshStructure(MeshingRequest request);
    }
}
