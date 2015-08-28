using System.Runtime.Serialization;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// The direction cosines of the first row and the first column with respect to the patient. See DICOM C.7.6.2.1.1.
    /// </summary>
    [DataContract]
    public class ImageOrientation
    {
        /// <summary>
        /// The direction cosines of the first row with respect to the patient. 
        /// </summary>
        [DataMember]
        public DirectionCosine Row 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The direction cosines of the first column with respect to the patient. 
        /// </summary>
        [DataMember]
        public DirectionCosine Column
        { 
            get; 
            set; 
        }
    };
}
