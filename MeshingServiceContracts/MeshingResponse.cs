using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.MeshingServiceContracts
{
    /// <summary>
    /// response contract with the result of the meshing
    /// </summary>
    [DataContract]
    public class MeshingResponse
    {
        /// <summary>
        /// indicates the original structure being meshed
        /// </summary>
        [DataMember]
        public Guid StructureGuid
        {
            get;
            set;
        }

        /// <summary>
        /// the resulting mesh
        /// </summary>
        [DataMember]
        public Guid SurfaceMeshGuid
        {
            get;
            set;
        }
    }
}
