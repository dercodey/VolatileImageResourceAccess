using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// represents a single image, possibly derived from a DICOM object
    /// </summary>
    [DataContract]
    public class ImageDataContract
    {
        #region Image Identification

        /// <summary>
        /// patient ID
        /// from DICOM <0010,0010>
        /// </summary>
        [DataMember]
        public string PatientId
        {
            get;
            set;
        }

        /// <summary>
        /// the DICOM series to which the image belongs
        /// </summary>
        [DataMember]
        public string SeriesInstanceUID
        {
            get;
            set;
        }

        /// <summary>
        /// the DICOM instance of the original image
        /// </summary>
        [DataMember]
        public string SOPInstanceUID
        {
            get;
            set;
        }

        /// <summary>
        /// internal image Guid
        /// </summary>
        [DataMember]
        public Guid ImageId
        {
            get;
            set;
        }

        /// <summary>
        /// a meaningful label
        /// </summary>
        [DataMember]
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// the series Frame Of Reference UID
        /// </summary>
        [DataMember]
        public string FrameOfReferenceUID
        {
            get;
            set;
        }

        #endregion Image Identification

        #region Image Geometry

        /// <summary>
        /// pixel spacing 
        /// from DICOM <xxxx,xxxx>
        /// in mm
        /// </summary>
        [DataMember]
        public VoxelSize PixelSpacing
        {
            get;
            set;
        }

        /// <summary>
        /// image position 
        /// from DICOM <xxxx,xxxx>
        /// in mm?
        /// </summary>
        [DataMember]
        public ImagePosition ImagePosition
        {
            get;
            set;
        }

        /// <summary>
        /// image direction cosines 
        /// from DICOM <xxxx,xxxx>
        /// </summary>
        [DataMember]
        public ImageOrientation ImageOrientation
        {
            get;
            set;
        }

        #endregion Image Geometry

        #region Image Pixels

        /// <summary>
        /// width (number of columns) of pixel buffer
        /// </summary>
        [DataMember]
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// height (number of rows) of pixel buffer
        /// </summary>
        [DataMember]
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// the shared buffer allowing access to pixels
        /// </summary>
        [DataMember]
        public SharedBuffer PixelBuffer
        {
            get;
            set;
        }

        #endregion Image Pixels
    }
}
