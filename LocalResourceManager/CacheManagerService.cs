using LocalResourceDataContracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LocalResourceManager
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, 
        ConfigurationName = "LargeNetNamedPipeBinding")]
    public class CacheManagerService : ICacheManagerService
    {
        public void CacheManagerService()
        {
            // check binding
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetPatientIds()
        {
            var patientIds = (from idc in _cacheImages.Values
                                 select idc.PatientId).Distinct();
            return patientIds.ToArray();
        }

        #region Image Operations

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid[] GetImageResourceIds(string patientId)
        {
            return (from idc in _cacheImages.Values
                    where idc.PatientId.CompareTo(patientId) == 0
                    select idc.Id).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUID"></param>
        /// <returns></returns>
        public Guid[] GetImageIdsBySeries(string seriesInstanceUID)
        {
            return (from idc in _cacheImages.Values
                    where idc.SeriesInstanceUID.CompareTo(seriesInstanceUID) == 0
                    select idc.Id).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ImageDataContract GetImage(Guid id)
        {
            return _cacheImages[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelType"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public ImageDataContract AddImage(ImageDataContract idc)
        {
            // assert that GUID was not already assigned
            System.Diagnostics.Trace.Assert(idc.Id.CompareTo(Guid.Empty) == 0);

            idc.Id = Guid.NewGuid();
            idc.PixelBuffer = 
                BufferRepository.CreateBuffer(idc.Id, typeof(ushort), idc.Width * idc.Height);

            _cacheImages.TryAdd(idc.Id, idc);

            return idc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveImage(Guid guid)
        {
            ImageDataContract idc;
            _cacheImages.TryRemove(guid, out idc);

            BufferRepository.FreeBuffer(idc.PixelBuffer.Id);
            //if (_cacheImages.Count == 0)
            //{
            //    System.Diagnostics.Trace.Assert(BufferRepository.GetCount() == 0);
            //}
        }

        // the image cache
        static ConcurrentDictionary<Guid, ImageDataContract> _cacheImages =
            new ConcurrentDictionary<Guid, ImageDataContract>();

        #endregion

        #region ImageVolume Operations

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid[] GetImageVolumeResourceIds(string patientId)
        {
            return (from ivdc in _cacheImageVolumes.Values
                    where ivdc.PatientId.CompareTo(patientId) == 0
                    select ivdc.Id).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUID"></param>
        /// <returns></returns>
        public ImageVolumeDataContract GetImageVolumeBySeriesInstanceUID(string seriesInstanceUID)
        {
            var ivdcs = from ivdc in _cacheImageVolumes.Values
                        where ivdc.SeriesInstanceUID.CompareTo(seriesInstanceUID) == 0
                        select ivdc;
            // System.Diagnostics.Trace.Assert(ivdcs.Count() <= 1);
            return ivdcs.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ImageVolumeDataContract GetImageVolume(Guid id)
        {
            return _cacheImageVolumes[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ivdc"></param>
        /// <returns></returns>
        public ImageVolumeDataContract AddImageVolume(ImageVolumeDataContract ivdc)
        {
            // assert that GUID was not already assigned
            System.Diagnostics.Trace.Assert(ivdc.Id.CompareTo(Guid.Empty) == 0);

            ivdc.Id = Guid.NewGuid();
            ivdc.PixelBuffer =
                BufferRepository.CreateBuffer(ivdc.Id, typeof(ushort), 
                ivdc.Width * ivdc.Height * ivdc.Depth);

            _cacheImageVolumes.TryAdd(ivdc.Id, ivdc);

            return ivdc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveImageVolume(Guid guid)
        {
            ImageVolumeDataContract ivdc;
            _cacheImageVolumes.TryRemove(guid, out ivdc);

            BufferRepository.FreeBuffer(ivdc.PixelBuffer.Id);
        }

        // the image cache
        static ConcurrentDictionary<Guid, ImageVolumeDataContract> _cacheImageVolumes =
            new ConcurrentDictionary<Guid, ImageVolumeDataContract>();

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid[] GetStructureResourceIds(string patientId)
        {
            return (from sdc in _cacheStructures.Values
                    where sdc.PatientId.CompareTo(patientId) == 0
                    select sdc.Id).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forUid"></param>
        /// <returns></returns>
        public Guid[] GetStructuresInFrameOfReferenceUid(string forUid)
        {
            var ids = from sdc in _cacheStructures.Values
                      where sdc.FrameOfReferenceUID.CompareTo(forUid) == 0
                      select sdc.Id;
            return ids.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StructureDataContract GetStructure(Guid id)
        {
            return _cacheStructures[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdc"></param>
        /// <returns></returns>
        public StructureDataContract AddStructure(StructureDataContract sdc)
        {
            System.Diagnostics.Trace.Assert(sdc.Id.CompareTo(Guid.Empty) == 0);
            sdc.Id = Guid.NewGuid();
            _cacheStructures.TryAdd(sdc.Id, sdc);
            return sdc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveStructure(Guid guid)
        {
            StructureDataContract sdc;
            _cacheStructures.TryRemove(guid, out sdc);
            foreach (var pdcGuid in sdc.Contours)
            {
                PolygonDataContract pdc;
                _cachePolygons.TryRemove(pdcGuid, out pdc);
                BufferRepository.FreeBuffer(pdc.VertexBuffer.Id);
            }
        }

        // 
        static ConcurrentDictionary<Guid, StructureDataContract> _cacheStructures =
            new ConcurrentDictionary<Guid, StructureDataContract>();


        public PolygonDataContract GetPolygon(Guid guid)
        {
            if (_cachePolygons.ContainsKey(guid))
                return _cachePolygons[guid];
            return null;
        }

        public PolygonDataContract AddPolygon(PolygonDataContract pdc)
        {
            System.Diagnostics.Trace.Assert(pdc.Id.CompareTo(Guid.Empty) == 0);
            pdc.Id = Guid.NewGuid();
            _cachePolygons.TryAdd(pdc.Id, pdc);

            // assert that GUID was not already assigned
            pdc.VertexBuffer =
                BufferRepository.CreateBuffer(pdc.Id, typeof(System.Windows.Media.Media3D.Vector3D),
                    pdc.VertexCount);

            return pdc;
        }

        static ConcurrentDictionary<Guid, PolygonDataContract> _cachePolygons =
            new ConcurrentDictionary<Guid, PolygonDataContract>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="relatedId"></param>
        /// <returns></returns>
        public SurfaceMeshDataContract GetSurfaceMeshByRelatedStructureId(Guid relatedId)
        {
            var smdcs = from smdc in _cacheMeshes.Values
                        where smdc.RelatedStructureId.CompareTo(relatedId) == 0
                        select smdc;
            System.Diagnostics.Trace.Assert(smdcs.Count() <= 1);
            return smdcs.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SurfaceMeshDataContract GetSurfaceMesh(Guid id)
        {
            return _cacheMeshes[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="smdc"></param>
        /// <returns></returns>
        public SurfaceMeshDataContract AddSurfaceMesh(SurfaceMeshDataContract smdc)
        {
            OperationContext oc = OperationContext.Current;
            oc.
            // assert that GUID was not already assigned
            System.Diagnostics.Trace.Assert(smdc.Id.CompareTo(Guid.Empty) == 0);
            smdc.Id = Guid.NewGuid();
            _cacheMeshes.TryAdd(smdc.Id, smdc);

            smdc.VertexBuffer =
                BufferRepository.CreateBuffer(Guid.NewGuid(), typeof(System.Windows.Media.Media3D.Vector3D),
                    smdc.VertexCount);

            smdc.NormalBuffer =
                BufferRepository.CreateBuffer(Guid.NewGuid(), typeof(System.Windows.Media.Media3D.Vector3D),
                    smdc.VertexCount);

            smdc.TriangleIndexBuffer =
                BufferRepository.CreateBuffer(Guid.NewGuid(), typeof(TriangleIndex),
                    smdc.TriangleCount);

            return smdc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveSurfaceMesh(Guid guid)
        {
            SurfaceMeshDataContract smdc;
            _cacheMeshes.TryRemove(guid, out smdc);
            BufferRepository.FreeBuffer(smdc.VertexBuffer.Id);
            BufferRepository.FreeBuffer(smdc.NormalBuffer.Id);
            BufferRepository.FreeBuffer(smdc.TriangleIndexBuffer.Id);
        }

        static ConcurrentDictionary<Guid, SurfaceMeshDataContract> _cacheMeshes =
            new ConcurrentDictionary<Guid, SurfaceMeshDataContract>();
    }
}
