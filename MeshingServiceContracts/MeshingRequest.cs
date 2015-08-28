using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.MeshingServiceContracts
{
    /// <summary>
    /// request to perform a meshing operation
    /// </summary>
    [DataContract]
    public class MeshingRequest
    {
        /// <summary>
        /// indicates the structure that is to serve as the input
        /// </summary>
        [DataMember]
        public Guid StructureGuid
        {
            get;
            set;
        }

        /// <summary>
        /// designate the list of contours for the structure that are to be meshed.
        /// TODO: is this really necessary?  this same information can be obtained through the
        /// Structure data contract
        /// </summary>
        [DataMember]
        public List<Guid> ContourGuids
        {
            get;
            set;
        }
    };
}
