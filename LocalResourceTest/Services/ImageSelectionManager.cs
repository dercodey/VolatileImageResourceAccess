using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Data;
using PheonixRt.MeshingServiceContracts;
using PheonixRt.ResampleServiceContracts;

using PheonixRt.Mvvm.LocalImageResourceServiceReference1;
using PheonixRt.Mvvm.LocalGeometryResourceServiceReference1;


namespace PheonixRt.Mvvm
{
    /// <summary>
    /// manager to handle display and selection of multiple images
    /// </summary>
    public class ImageSelectionManager
    {
        Dispatcher _dispatcher;

        public ImageSelectionManager(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            ICollectionView pgv = CollectionViewSource.GetDefaultView(_patientGroups);
            pgv.SortDescriptions.Add(new SortDescription("PatientId", ListSortDirection.Ascending));

            ICollectionView seriesVm = CollectionViewSource.GetDefaultView(_series);
            seriesVm.SortDescriptions.Add(new SortDescription("SeriesLabel", ListSortDirection.Ascending));

            ICollectionView structureVm = CollectionViewSource.GetDefaultView(_structures);
            structureVm.SortDescriptions.Add(new SortDescription("FrameOfReferenceUID", ListSortDirection.Ascending));
            structureVm.SortDescriptions.Add(new SortDescription("ROIName", ListSortDirection.Ascending));

        }


        public void RefreshAll()
        {
            LocalImageResourceManagerClient imageResource = new LocalImageResourceManagerClient();
            try
            {
                var patIds = imageResource.GetPatientIds();
                _patientGroups.Clear();

                patIds.ForEach(
                    id => _patientGroups.Add(new PatientGroupViewModel(id)));
            }
            finally
            {
                imageResource.Close();
            }
        }

        /// <summary>
        /// upon image stored, update the image series vm
        /// </summary>
        /// <param name="methodGuid"></param>
        /// <param name="imageGuid"></param>
        /// <param name="repoGb"></param>
        void ImageStoredResponse_ImageStoredEvent(string methodGuid, Guid imageGuid, double repoGb)
        {
            LocalImageResourceManagerClient imageResource = new LocalImageResourceManagerClient();
            try
            {
                var idc = imageResource.GetImage(imageGuid);

                // TODO: null happens because of orphaned events
                if (idc != null)
                {
                    _dispatcher.Invoke(() =>
                    {
                        AddOrUpdate(_patientGroups,
                            pg => pg.PatientId.CompareTo(idc.PatientId) == 0,
                            pg => pg.InstanceCount++,
                            () => new PatientGroupViewModel(idc.PatientId));

                        ICollectionView pgv = CollectionViewSource.GetDefaultView(_patientGroups);
                        var pgvm = (PatientGroupViewModel)pgv.CurrentItem;
                        if (pgvm != null
                            && pgvm.PatientId.CompareTo(idc.PatientId) == 0)
                        {
                            AddOrUpdate<ImageSeriesViewModel>(_series,
                                s => s.SeriesInstanceUID.CompareTo(idc.SeriesInstanceUID) == 0,
                                s => s.InstanceCount++,
                                () => ImageSeriesViewModel.Create(idc));
                        }
                    });
                }
            }
            finally
            { 
                imageResource.Close();
            }
        }

        /// <summary>
        /// upon structure stored, update the structure vm collection
        /// </summary>
        /// <param name="methodID">the scan method invoke</param>
        /// <param name="response">response structure</param>
        void ImageStoredResponse_StructureStoredEvent(string methodID, Guid structureGuid)
        {
            LocalGeometryResourceManagerClient geometryResource = new LocalGeometryResourceManagerClient();
            var sdc = geometryResource.GetStructure(structureGuid);

            _dispatcher.Invoke(() =>
            {
                ICollectionView pgv = CollectionViewSource.GetDefaultView(_patientGroups);
                var pgvm = (PatientGroupViewModel)pgv.CurrentItem;
                if (pgvm != null
                    && pgvm.PatientId.CompareTo(sdc.PatientId) == 0)
                {
                    AddOrUpdate<StructureViewModel>(_structures,
                        s => s.ROIName.CompareTo(sdc.ROIName) == 0,
                        s => s.ROICount++,
                        () => new StructureViewModel(sdc.Id, sdc.ROIName)
                        {
                            FrameOfReferenceUID = sdc.FrameOfReferenceUID,
                        });
                }
            });

            if (sdc.Contours.Count < 2)
                return;
        }

        /// <summary>
        /// upon meshing complete, update the structure vms
        /// </summary>
        /// <param name="methodID">the meshing method invoke</param>
        /// <param name="response">response structure</param>
        void MeshingCompleteResponse_MeshCompleteEvent(string methodId, MeshingResponse response)
        {
            var lgrm = new LocalGeometryResourceManagerClient();

            if (_structures.Any(s => s.Id.CompareTo(response.StructureGuid) == 0))
            {
                _dispatcher.Invoke(() => 
                {
                    var svm = from vm in _structures
                              where vm.Id.CompareTo(response.StructureGuid) == 0
                              select vm;

                    var smdc = lgrm.GetSurfaceMesh(response.SurfaceMeshGuid);
                    System.Diagnostics.Trace.Assert(smdc != null);

                    svm.First().MeshStatus = string.Format("Meshed ({0} vertices)",
                        (int)smdc.VertexCount);
                });
            }

            lgrm.Close();
        }

        /// <summary>
        /// upone resample, update the vms for the image series
        /// </summary>
        /// <param name="methodID">the resample method invoke</param>
        /// <param name="response">response structure</param>
        void ResampleDoneResponse_ResampleDoneEvent(string methodID, ImageVolumeResampleResponse response)
        {
            var lirm = new LocalImageResourceManagerClient();

            _dispatcher.Invoke(() =>
            {
                // now delete the images
                var guids = lirm.GetImageIdsBySeries(response.SeriesInstanceUID);
                foreach (var guid in guids)
                {
                    lirm.RemoveImage(guid);
                }

                if (_series.Any(s => s.SeriesInstanceUID.CompareTo(response.SeriesInstanceUID) == 0))
                {
                    var svm = from vm in _series
                              where vm.SeriesInstanceUID.CompareTo(response.SeriesInstanceUID) == 0
                              select vm;
                    svm.First().ResampleStatus =
                        string.Format("Resampled ({0} slices)", (int)guids.Count);
                }
            });

            lirm.Close();
        }

        // vm for collection of patient objects
        public ObservableCollection<PatientGroupViewModel> _patientGroups =
            new ObservableCollection<PatientGroupViewModel>();

        // vm for collection of images series'
        public ObservableCollection<ImageSeriesViewModel> _series =
            new ObservableCollection<ImageSeriesViewModel>();

        // vm for collection of structures
        public ObservableCollection<StructureViewModel> _structures =
            new ObservableCollection<StructureViewModel>();

        public static void AddOrUpdate<T>(ObservableCollection<T> collection,
            Func<T, bool> matchFunc,
            Action<T> updateFunc,
            Func<T> createFunc)
        {
            if (collection.Any(matchFunc))
            {
                var match = from t in collection
                            where
                                matchFunc(t)
                            select t;
                updateFunc(match.First());
            }
            else
            {
                T newT = createFunc();
                collection.Add(newT);
            }
        }
    }
}
