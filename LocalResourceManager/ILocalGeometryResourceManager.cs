using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using PheonixRt.DataContracts;

namespace LocalResourceManager
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface ILocalGeometryResourceManager
    {
        #region Structure Operations

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        [OperationContract]
        Guid[] GetStructureResourceIds(string patientId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forUid"></param>
        /// <returns></returns>
        [OperationContract]
        Guid[] GetStructuresInFrameOfReferenceUid(string forUid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        StructureDataContract GetStructure(Guid id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdc"></param>
        /// <returns></returns>
        [OperationContract]
        StructureDataContract AddStructure(StructureDataContract sdc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        [OperationContract]
        void RemoveStructure(Guid guid);

        #endregion

        #region Polygon Operations

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdc"></param>
        /// <returns></returns>
        [OperationContract]
        ContourDataContract AddPolygon(ContourDataContract pdc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [OperationContract]
        ContourDataContract GetPolygon(Guid guid);

        #endregion

        #region Mesh Operations

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        SurfaceMeshDataContract GetSurfaceMesh(Guid id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relatedId"></param>
        /// <returns></returns>
        [OperationContract]
        SurfaceMeshDataContract GetSurfaceMeshByRelatedStructureId(Guid relatedId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdc"></param>
        /// <returns></returns>
        [OperationContract]
        SurfaceMeshDataContract AddSurfaceMesh(SurfaceMeshDataContract sdc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        [OperationContract]
        void RemoveSurfaceMesh(Guid guid);

        #endregion

    }
}
