using System.Runtime.Serialization;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// The x, y, and z coordinates of the upper left hand corner (center of the first voxel) of the image. See DICOM C.7.6.2.1.1.
    /// </summary>
    [DataContract]
    public class ImagePosition
    {
        /// <summary>
        /// The x coordinate of the upper left hand corner (center of the first voxel) of the image.
        /// </summary>
        /// <return>Millimeter</return>
        [DataMember]
        public float X 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The y coordinate of the upper left hand corner (center of the first voxel) of the image. 
        /// </summary>
        /// <return>Millimeter</return>
        [DataMember]
        public float Y 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The z coordinate of the upper left hand corner (center of the first voxel) of the image. 
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
