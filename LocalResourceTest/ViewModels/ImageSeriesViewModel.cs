using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PheonixRt.DataContracts;

namespace PheonixRt.Mvvm
{
    /// <summary>
    /// view model appropriate for displaying metadata about an image series
    /// </summary>
    public class ImageSeriesViewModel : BindableBase
    {
        /// <summary>
        /// create for the series containing a single image
        /// </summary>
        /// <param name="idc">the data contract for the image</param>
        /// <returns>the new view model</returns>
        public static ImageSeriesViewModel Create(ImageDataContract idc)
        {
            var isvm = new ImageSeriesViewModel()
            {
                SeriesInstanceUID = idc.SeriesInstanceUID,
                FrameOfReferenceUID = idc.FrameOfReferenceUID,
                SeriesLabel = idc.Label,
            };
            return isvm;
        }

        /// <summary>
        /// create for the series for an image volume
        /// </summary>
        /// <param name="ivdc">the image volume data contract</param>
        /// <returns>the new view model</returns>
        public static ImageSeriesViewModel Create(UniformImageVolumeDataContract ivdc)
        {
            var isvm = new ImageSeriesViewModel()
            {
                SeriesInstanceUID = ivdc.Identity.SeriesInstanceUID,
                FrameOfReferenceUID = ivdc.FrameOfReferenceUID,
                SeriesLabel = ivdc.Identity.Label,
            };
            return isvm;
        }

        /// <summary>
        /// returns the image series' DICOM Series Instance UID
        /// </summary>
        public string SeriesInstanceUID
        {
            get { return _seriesInstanceUID; }
            set { SetProperty(ref _seriesInstanceUID, value); }
        }
        string _seriesInstanceUID;

        /// <summary>
        /// returns the FrameOfReference UID for the series, truncated
        /// </summary>
        public string FrameOfReferenceUID
        {
            get { return _forUid; }
            set
            {
                string truncated = TruncateUID(value);
                SetProperty(ref _forUid, truncated);
            }
        }
        string _forUid;

        /// <summary>
        /// truncates a UID, to replace the middle numbers with ...
        /// </summary>
        /// <param name="uid">the UID to be truncated</param>
        /// <returns>the truncated UID</returns>
        public static string TruncateUID(string uid)
        {
            if (uid == null
                || uid.CompareTo(string.Empty) == 0)
                return "<null>";

            return string.Format("{0}...{1}",
                uid.Substring(0, 7),
                uid.Substring(uid.Length - 7, 7));
        }

        /// <summary>
        /// the series label
        /// </summary>
        public string SeriesLabel
        {
            get { return _seriesLabel; }
            set { SetProperty(ref _seriesLabel, value); }
        }
        string _seriesLabel;

        /// <summary>
        /// count of instances in the series
        /// </summary>
        public int InstanceCount
        {
            get { return _instanceCount; }
            set { SetProperty(ref _instanceCount, value); }
        }
        int _instanceCount = 1;

        /// <summary>
        /// the resample status
        /// </summary>
        public string ResampleStatus
        {
            get { return _resampleStatus; }
            set { SetProperty(ref _resampleStatus, value); }
        }
        string _resampleStatus = "<not resampled>";
    }
}
