using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using PheonixRt.DataContracts;
using PheonixRt.MeshingServiceContracts;
using PheonixRt.ResampleServiceContracts;

using PheonixRt.Mvvm.LocalImageResourceServiceReference1;
using PheonixRt.Mvvm.LocalGeometryResourceServiceReference1;

using PheonixRt.Mvvm.Services;

namespace PheonixRt.Mvvm
{
    public class ImageDisplayManager
    {
        public ImageDisplayManager()
        {
            ICollectionView pgv = CollectionViewSource.GetDefaultView(_patientGroups);
            pgv.SortDescriptions.Add(new SortDescription("PatientId", ListSortDirection.Ascending));

            ICollectionView seriesVm = CollectionViewSource.GetDefaultView(_series);
            seriesVm.SortDescriptions.Add(new SortDescription("SeriesLabel", ListSortDirection.Ascending));

            ICollectionView structureVm = CollectionViewSource.GetDefaultView(_structures);
            structureVm.SortDescriptions.Add(new SortDescription("FrameOfReferenceUID", ListSortDirection.Ascending));
            structureVm.SortDescriptions.Add(new SortDescription("ROIName", ListSortDirection.Ascending));

            DicomLoaderManagerHelper.ImageStoredEvent += ImageStoredResponse_ImageStoredEvent;
            DicomLoaderManagerHelper.StructureStoredEvent += ImageStoredResponse_StructureStoredEvent;

            MeshingManagerHelper.MeshCompleteEvent += MeshingCompleteResponse_MeshCompleteEvent;

            ResampleDoneResponse.ResampleDoneEvent += ResampleDoneResponse_ResampleDoneEvent;
        }

        void ImageStoredResponse_ImageStoredEvent(string methodGuid, Guid imageGuid, double repoGb)
        {
            LocalImageResourceManagerClient imageResource = new LocalImageResourceManagerClient();
            var idc = imageResource.GetImage(imageGuid);

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

            imageResource.Close();
        }

        void ImageStoredResponse_StructureStoredEvent(string methodID, Guid structureGuid)
        {
            LocalGeometryResourceManagerClient geometryResource = new LocalGeometryResourceManagerClient();
            var sdc = geometryResource.GetStructure(structureGuid);

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

            if (sdc.Contours.Count < 2)
                return;
        }

        void MeshingCompleteResponse_MeshCompleteEvent(string methodId, MeshingResponse response)
        {
            var lgrm = new LocalGeometryResourceManagerClient();

            if (_structures.Any(s => s.Id.CompareTo(response.StructureGuid) == 0))
            {
                var svm = from vm in _structures
                          where vm.Id.CompareTo(response.StructureGuid) == 0
                          select vm;

                var smdc = lgrm.GetSurfaceMesh(response.SurfaceMeshGuid);
                System.Diagnostics.Trace.Assert(smdc != null);

                svm.First().MeshStatus = string.Format("Meshed ({0} vertices)", 
                    (int)smdc.VertexCount);
            }

            lgrm.Close();
        }

        void ResampleDoneResponse_ResampleDoneEvent(string methodID, ImageVolumeResampleResponse response)
        {
            var lirm = new LocalImageResourceManagerClient();

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

            lirm.Close();
        }

        public ObservableCollection<PatientGroupViewModel> _patientGroups =
            new ObservableCollection<PatientGroupViewModel>();

        public ObservableCollection<ImageSeriesViewModel> _series =
            new ObservableCollection<ImageSeriesViewModel>();

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
