using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.DataContracts;

using PheonixRt.MeshingServiceContracts;

namespace PheonixRt.MeshingEngineService
{
    /// <summary>
    /// service interface for a meshing engine
    /// </summary>
    [ServiceContract]
    public interface IMeshingEngine
    {
        /// <summary>
        /// synchronous operation to perform meshing for a single request
        /// </summary>
        /// <param name="request">indicates the structure to be meshed</param>
        /// <returns>the response contract indicates the mesh created</returns>
        [OperationContract]
        MeshingResponse MeshStructure(MeshingRequest request);
    }
}
