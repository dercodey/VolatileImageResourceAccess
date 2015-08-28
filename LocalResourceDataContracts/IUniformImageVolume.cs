using System;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// Interface abstracting the IUniformImageVolume
    /// </summary>
    public interface IUniformImageVolume
    {
        #region Volume Identification

        /// <summary>
        /// Volume identity object
        /// </summary> 
        VolumeIdentity Identity { get; set; }

        #endregion Volume Identification

        #region Volume Geometry

        /// <summary>
        /// Represents the frame-of-reference within which the voxels are defined
        /// </summary>
        string FrameOfReferenceUID { get; set; }

        /// <summary>
        /// VoxelSpacing -  from DICOM Pixel Spacing (0028,0030) in mm
        /// </summary>
        VoxelSize VoxelSpacing { get; set; }

        /// <summary>
        /// ImagePosition - from DICOM Image Position (Patient) (0028,0032) in mm
        /// </summary>
        ImagePosition ImagePosition { get; set; }

        /// <summary>
        /// VolumeOrientation, relating the Index coordinate system to the DICOM Patient Coordinate System
        /// from DICOM Image Orientation (Patient) (0020, 0037)
        /// direction cosines are unit vectors
        /// </summary>
        VolumeOrientation VolumeOrientation { get; set; }

        /// <summary>
        /// Image Isocenter: origin of IEC Patient Coordinate System in the DICOM Patient Coordinate System
        /// In mm, to be consistent with ImagePosition?  SHOULD THIS REALLY BE HERE?
        /// </summary>
        ImagePosition Isocenter { get; set; }

        #endregion Volume Geometry

        #region Voxel Access

        /// <summary>
        /// width of each row in pixels
        /// </summary>
        int Width { get; set; }
        
        /// <summary>
        /// height of each slice in pixels
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// depth of the volume
        /// </summary>
        int Depth { get; set; }

        /// <summary>
        /// access to the pixels via a shared buffer
        /// </summary>
        SharedBuffer PixelBuffer { get; set; }

        #endregion VoxelAccess
    }
}
