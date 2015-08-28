//using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.Mvvm
{
    /// <summary>
    /// view model to represent meta data for a structure
    /// </summary>
    public class StructureViewModel : BindableBase
    {
        /// <summary>
        /// constructs a new StructureViewModel
        /// </summary>
        /// <param name="id">the unique ID for the structure</param>
        /// <param name="roiName">the name of the ROI</param>
        public StructureViewModel(Guid id, string roiName)
        {
            Id = id;
            ROIName = roiName;
        }

        /// <summary>
        /// the ID for the structure
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
        Guid _id;

        /// <summary>
        /// the FOR UID within which the structure is defined
        /// </summary>
        public string FrameOfReferenceUID
        {
            get { return _forUid; }
            set
            {
                string truncated = ImageSeriesViewModel.TruncateUID(value); 
                SetProperty(ref _forUid, truncated);
            }
        }
        string _forUid;

        /// <summary>
        /// the name of the ROI
        /// </summary>
        public string ROIName
        {
            get { return _roiName; }
            set { SetProperty(ref _roiName, value);  }
        }
        string _roiName;

        /// <summary>
        /// display string for the current mesh status
        /// </summary>
        public string MeshStatus
        {
            get { return _meshStatus; }
            set { SetProperty(ref _meshStatus, value); }
        }
        string _meshStatus;

        /// <summary>
        /// count of ROIs in the structure
        /// </summary>
        public int ROICount
        {
            get { return _roiCount; }
            set { SetProperty(ref _roiCount, value); }
        }
        int _roiCount = 1;
    }
}
