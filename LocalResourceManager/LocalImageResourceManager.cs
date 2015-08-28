using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Channels;

using ServiceModelEx.Transactional;

using PheonixRt.DataContracts;

namespace LocalResourceManager
{
    /// <summary>
    /// represents an instance of the LIRM (containing a static dictionary to hold on to 
    /// the represented objects)
    /// currently also handles geometry, because this hasn't been moved out to the other service
    /// </summary>
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
    public class LocalImageResourceManager : ILocalImageResourceManager
    {
        #region Image Operations

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetImageResourceIds(string patientId)
        {
            return (from idc in _cacheImages.Values
                    where idc.PatientId != null
                        && idc.PatientId.CompareTo(patientId) == 0
                    select idc.ImageId).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUID"></param>
        /// <returns></returns>
        public List<Guid> GetImageIdsBySeries(string seriesInstanceUID)
        {
            return (from idc in _cacheImages.Values
                    where idc.SeriesInstanceUID != null
                        && idc.SeriesInstanceUID.CompareTo(seriesInstanceUID) == 0
                    select idc.ImageId).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ImageDataContract GetImage(Guid id)
        {
            if (_cacheImages.ContainsKey(id))
                return _cacheImages[id];
            return null;
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
            System.Diagnostics.Trace.Assert(idc.ImageId.CompareTo(Guid.Empty) == 0);

            idc.ImageId = Guid.NewGuid();
            idc.PixelBuffer =
                BufferRepository.CreateBuffer(idc.ImageId, typeof(ushort), idc.Width * idc.Height);

            _cacheImages.TryAdd(idc.ImageId, idc);

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
        public List<Guid> GetImageVolumeResourceIds(string patientId)
        {
            return (from ivdc in _cacheImageVolumes.Values
                    where ivdc.Identity.PatientId.CompareTo(patientId) == 0
                    select ivdc.Identity.Guid).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seriesInstanceUID"></param>
        /// <returns></returns>
        public UniformImageVolumeDataContract GetImageVolumeBySeriesInstanceUID(string seriesInstanceUID)
        {
            var ivdcs = from ivdc in _cacheImageVolumes.Values
                        where ivdc.Identity.SeriesInstanceUID.CompareTo(seriesInstanceUID) == 0
                        select ivdc;
            return ivdcs.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UniformImageVolumeDataContract GetImageVolume(Guid id)
        {
            //using (new OperationContextScope(OperationContext.Current.Channel))
            {
                int nHeaderIn = OperationContext.Current.IncomingMessageHeaders.FindHeader("Test", "http://tempura.org");
                if (nHeaderIn >= 0)
                { 
                    var testHeader = OperationContext.Current.IncomingMessageHeaders[nHeaderIn];
                }
                //var header = MessageHeader.CreateHeader("StreamShared", "http://tempura.org", "0000-000000-000");
                //OperationContext.Current.OutgoingMessageHeaders.Add(header);
            }
            return _cacheImageVolumes[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ivdc"></param>
        /// <returns></returns>
        public UniformImageVolumeDataContract AddImageVolume(UniformImageVolumeDataContract ivdc)
        {
            // assert that GUID was not already assigned
            System.Diagnostics.Trace.Assert(ivdc.Identity.Guid.CompareTo(Guid.Empty) == 0);

            ivdc.Identity.Guid= Guid.NewGuid();
            ivdc.PixelBuffer =
                BufferRepository.CreateBuffer(ivdc.Identity.Guid, typeof(ushort), 
                ivdc.Width * ivdc.Height * ivdc.Depth);

            _cacheImageVolumes.Add(ivdc.Identity.Guid, ivdc);

            return ivdc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveImageVolume(Guid guid)
        {
            UniformImageVolumeDataContract ivdc;
            if (_cacheImageVolumes.TryGetValue(guid, out ivdc))
            {
                _cacheImageVolumes.Remove(guid);
                BufferRepository.FreeBuffer(ivdc.PixelBuffer.Id);
            }
        }

        // the image cache
        static TransactionalDictionary<Guid, UniformImageVolumeDataContract> _cacheImageVolumes =
            new TransactionalDictionary<Guid, UniformImageVolumeDataContract>();

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetPatientIds()
        {
            var patientIds = (from idc in _cacheImages.Values
                              select idc.PatientId).Distinct();
            return patientIds.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetRepositorySizeGB()
        {
            return BufferRepository.GetSizeBytes() / 1e+9;
        }

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
#if SLOW_POKE
                                var handle = sbLocal.GetHandle();
                                const long STEP_SIZE = 4096;
                                long numSteps = (long)handle.ByteLength / STEP_SIZE;
                                System.Threading.Tasks.Parallel.For(0, numSteps,
                                    at => handle.Read<byte>((ulong)(at * STEP_SIZE)));
                                sbLocal.ReleaseHandle();
#else
                                    BufferRepository.ReadFile(sbLocal.Id);
#endif
                                    _sbDone.Add(sbLocal);
                                }
                                else
                                {
                                    System.Threading.Thread.Sleep(1000);
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

    }
}
