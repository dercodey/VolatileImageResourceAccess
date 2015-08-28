using System.Runtime.Serialization;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// Class represents the size of a voxel
    /// </summary>
    [DataContract]
    public class VoxelSize
    {
        /// <summary>
        /// Size in x dimension
        /// </summary>
        [DataMember]
        public float X { get; set; }

        /// <summary>
        /// Size in y dimension
        /// </summary>
        [DataMember]
        public float Y { get; set; }

        /// <summary>
        /// Size in z dimension
        /// </summary>
        [DataMember]
        public float Z { get; set; }
    }
}
