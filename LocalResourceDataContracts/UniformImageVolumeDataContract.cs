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
    public class UniformImageVolumeDataContract : IUniformImageVolume
    {
        public UniformImageVolumeDataContract()
        {
            Identity = new VolumeIdentity();
        }

        #region Volume Identification

        /// <summary>
        /// Volume identity
        /// </summary> 
        [DataMember]
        public VolumeIdentity Identity 
        { 
            get; 
            set; 
        }

        #endregion Volume Identification

        #region Volume Geometry

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FrameOfReferenceUID
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public VoxelSize VoxelSpacing
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public ImagePosition ImagePosition
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public VolumeOrientation VolumeOrientation
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public ImagePosition Isocenter
        {
            get;
            set;
        }

        #endregion Volume Geometry

        #region Voxel Access

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int Height
        {
            get;
            set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int Depth
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public SharedBuffer PixelBuffer
        {
            get;
            set;
        }

        #endregion Voxel Access
    }
}
