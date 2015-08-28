using System.Runtime.Serialization;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// The direction cosines of the volume with respect to the DICOM patient coordinate system. See DICOM C.7.6.2.1.1.
    /// </summary>
    [DataContract]
    public class VolumeOrientation
    {
        /// <summary>
        /// The direction cosines of the first row (moving in X direction) 
        /// with respect to the patient. 
        /// </summary>
        [DataMember]
        public DirectionCosine Row 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The direction cosines of the first column (moving in Y direction) 
        /// with respect to the patient. 
        /// </summary>
        [DataMember]
        public DirectionCosine Column
        {
            get;
            set;
        }

        /// <summary>
        /// The direction cosines in slice direction (moving in voxel Z direction) 
        /// w.r.t. DICOM Patient coordinate system
        /// </summary>
        [DataMember]
        public DirectionCosine Depth
        {
            get;
            set;
        }
    };
}
