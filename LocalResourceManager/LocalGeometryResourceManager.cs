using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.DataContracts;
using ServiceModelEx.Transactional;

namespace LocalResourceManager
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
    public class LocalGeometryResourceManager : ILocalGeometryResourceManager
    {
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
            _cacheStructures.Add(sdc.Id, sdc);
            return sdc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveStructure(Guid guid)
        {
            StructureDataContract sdc;
            if (_cacheStructures.TryGetValue(guid, out sdc))
            {
                _cacheStructures.Remove(guid);
                foreach (var pdcGuid in sdc.Contours)
                {
                    ContourDataContract pdc;
                    if (_cachePolygons.TryGetValue(pdcGuid, out pdc))
                    {
                        _cachePolygons.Remove(pdcGuid);
                        BufferRepository.FreeBuffer(pdc.VertexBuffer.Id);
                    }
                }
            }
        }

        // 
        static TransactionalDictionary<Guid, StructureDataContract> _cacheStructures =
            new TransactionalDictionary<Guid, StructureDataContract>();


        public ContourDataContract GetPolygon(Guid guid)
        {
            if (_cachePolygons.ContainsKey(guid))
                return _cachePolygons[guid];
            return null;
        }

        public ContourDataContract AddPolygon(ContourDataContract pdc)
        {
            System.Diagnostics.Trace.Assert(pdc.Id.CompareTo(Guid.Empty) == 0);
            pdc.Id = Guid.NewGuid();
            _cachePolygons.Add(pdc.Id, pdc);

            // assert that GUID was not already assigned
            pdc.VertexBuffer =
                BufferRepository.CreateBuffer(pdc.Id, typeof(System.Windows.Media.Media3D.Vector3D),
                    pdc.VertexCount);

            return pdc;
        }

        static TransactionalDictionary<Guid, ContourDataContract> _cachePolygons =
            new TransactionalDictionary<Guid, ContourDataContract>();


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
            // assert that GUID was not already assigned
            System.Diagnostics.Trace.Assert(smdc.Id.CompareTo(Guid.Empty) == 0);
            smdc.Id = Guid.NewGuid();
            _cacheMeshes.Add(smdc.Id, smdc);

            smdc.VertexBuffer =
                BufferRepository.CreateBuffer(Guid.NewGuid(), typeof(System.Windows.Media.Media3D.Vector3D),
                    smdc.VertexCount);

            smdc.NormalBuffer =
                BufferRepository.CreateBuffer(Guid.NewGuid(), typeof(System.Windows.Media.Media3D.Vector3D),
                    smdc.VertexCount);

            if (smdc.TriangleCount > 0)
            { 
                smdc.TriangleIndexBuffer =
                    BufferRepository.CreateBuffer(Guid.NewGuid(), typeof(TriangleIndex),
                        smdc.TriangleCount);
            }

            return smdc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveSurfaceMesh(Guid guid)
        {
            SurfaceMeshDataContract smdc;
            if (_cacheMeshes.TryGetValue(guid, out smdc))
            {
                _cacheMeshes.Remove(guid);
                BufferRepository.FreeBuffer(smdc.VertexBuffer.Id);
                BufferRepository.FreeBuffer(smdc.NormalBuffer.Id);
                BufferRepository.FreeBuffer(smdc.TriangleIndexBuffer.Id);
            }
        }

        static TransactionalDictionary<Guid, SurfaceMeshDataContract> _cacheMeshes =
            new TransactionalDictionary<Guid, SurfaceMeshDataContract>();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetPatientIds()
        {
            var patientIds = (from idc in _cacheStructures.Values
                              select idc.PatientId).Distinct();
            return patientIds.ToArray();
        }

#if GEOMETRY_PREFETCH
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        public void PrefetchBuffer(SharedBuffer sb)
        {
            if ((!_sbStack.Any(sbOther => sbOther.Id.CompareTo(sb.Id) == 0))
                && (!_sbDone.Any(sbOther => sbOther.Id.CompareTo(sb.Id) == 0)))
                _sbStack.Push(sb);

            if (_sbPrefetchTask == null
                || _sbPrefetchTask.Status != System.Threading.Tasks.TaskStatus.Running)
            {
                Action prefetchAction = () =>
                    {
                        try
                        {
                            while (true)
                            {
                                SharedBuffer sbLocal;
                                if (_sbStack.TryPop(out sbLocal))
                                {
//#if SLOW_POKE
//                                var handle = sbLocal.GetHandle();
//                                const long STEP_SIZE = 4096;
//                                long numSteps = (long)handle.ByteLength / STEP_SIZE;
//                                System.Threading.Tasks.Parallel.For(0, numSteps,
//                                    at => handle.Read<byte>((ulong)(at * STEP_SIZE)));
//                                sbLocal.ReleaseHandle();
//#else
//                                    BufferRepository.ReadFile(sbLocal.Id);
//#endif
                                    _sbDone.Add(sbLocal);
                                }
                            }
                        }
                        finally
                        { 
                            System.Console.WriteLine("Exiting prefetch...");
                        }
                    };

                _sbPrefetchTask = new System.Threading.Tasks.Task(prefetchAction);
                _sbPrefetchTask.Start();
            }
        }

        public void ClearPrefetchStack()
        {
            _sbStack.Clear();
            _sbDone = new ConcurrentBag<SharedBuffer>();
        }

        static System.Threading.Tasks.Task _sbPrefetchTask;

        static ConcurrentStack<SharedBuffer> _sbStack =
            new ConcurrentStack<SharedBuffer>();

        static ConcurrentBag<SharedBuffer> _sbDone =
            new ConcurrentBag<SharedBuffer>();
#endif
    }
}
