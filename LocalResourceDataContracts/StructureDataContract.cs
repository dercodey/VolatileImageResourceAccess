using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class StructureDataContract
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PatientId
        {
            get;
            set;
        }

        /// <summary>
        /// for structures originating in a DICOM structure set, this is the SeriesInstanceUID of that structure set
        /// </summary>
        [DataMember]
        public string SeriesInstanceUID
        {
            get;
            set;
        }

        /// <summary>
        /// for structures originating in a DICOM structure set, this is the SOPInstanceUID of that structure set
        /// </summary>
        [DataMember]
        public string SOPInstanceUID
        {
            get;
            set;
        }

        /// <summary>
        /// for structures originating in a DICOM structure set, this is the StructureSetLabel of that structure set
        /// </summary>
        [DataMember]
        public string StructureSetLabel
        {
            get;
            set;
        }

        /// <summary>
        /// unique ID for this structure in the system
        /// </summary>
        [DataMember]
        public Guid Id
        {
            get;
            set;
        }

        /// <summary>
        /// meaningful name; not guaranteed unique
        /// </summary>
        [DataMember]
        public string ROIName
        {
            get;
            set;
        }

        /// <summary>
        /// represents the original Frame Of Reference within which the structure is defined
        /// </summary>
        [DataMember]
        public string FrameOfReferenceUID
        {
            get;
            set;
        }

        /// <summary>
        /// associated contours as Polygons
        /// </summary>
        [DataMember]
        public List<Guid> Contours
        {
            get;
            set;
        }
    }
}
