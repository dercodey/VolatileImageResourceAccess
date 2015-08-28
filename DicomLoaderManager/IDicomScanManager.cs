using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DicomLoaderManager
{
    /// <summary>
    /// Service contract for a dicom scan manager, that scans a particular path
    /// recursively and stores the results in local storage
    /// </summary>
    [ServiceContract]
    public interface IDicomScanManager
    {
        /// <summary>
        /// Begin scanning of the directory.  Set flag to "rescan" to continue scanning/
        /// </summary>
        /// <param name="pathname">root path for the scan</param>
        /// <param name="rescan">flag to indicate to scan once and quit, or continue</param>
        [OperationContract(IsOneWay=true)]
        void ScanDirectory(string pathname, bool rescan);
    }
}
