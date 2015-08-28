using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.DataContracts;

namespace LocalResourceManager
{
    [ServiceContract]
    public interface ILocalImageResourceManager
    {
        #region Image Operations

        [OperationContract]
        List<Guid> GetImageResourceIds(string patientId);

        [OperationContract]
        List<Guid> GetImageIdsBySeries(string seriesInstanceUID);

        [OperationContract]
        ImageDataContract GetImage(Guid id);

        [OperationContract]
        ImageDataContract AddImage(ImageDataContract idc);

        [OperationContract]
        void RemoveImage(Guid guid);

        #endregion

        #region ImageVolume Operations

        [OperationContract]
        List<Guid> GetImageVolumeResourceIds(string patientId);

        [OperationContract]
        UniformImageVolumeDataContract GetImageVolume(Guid id);

        [OperationContract]
        UniformImageVolumeDataContract GetImageVolumeBySeriesInstanceUID(string seriesInstanceUID);

        [OperationContract]
        UniformImageVolumeDataContract AddImageVolume(UniformImageVolumeDataContract idc);

        [OperationContract]
        void RemoveImageVolume(Guid guid);

        #endregion

        #region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<string> GetPatientIds();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        double GetRepositorySizeGB();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        [OperationContract(IsOneWay = true)]
        void PrefetchBuffer(SharedBuffer sb);

        [OperationContract(IsOneWay = true)]
        void ClearPrefetchStack();

        #endregion
    }
}
