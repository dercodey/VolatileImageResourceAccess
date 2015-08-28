using System.Runtime.Serialization;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// Specifies the direction cosine relative to the patient. Follows the DICOM standard representation, C.7.6.2.1.1.
    /// </summary>
    [DataContract]
    public class DirectionCosine
    {
        /// <summary>
        /// The x coordinate of the direction cosine relative to the patient
        /// </summary>
        /// <return>Millimeter</return>
        [DataMember]
        public float X 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The y coordinate of the direction cosine relative to the patient
        /// </summary>
        /// <return>Millimeter</return>
        [DataMember]
        public float Y 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The z coordinate of the direction cosine relative to the patient
        /// </summary>
        /// <return>Millimeter</return>
        [DataMember]
        public float Z 
        { 
            get; 
            set; 
        }
    };
}
