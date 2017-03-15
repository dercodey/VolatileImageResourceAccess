using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

using System.Windows;
using System.Windows.Data;

using PheonixRt.Mvvm.LocalImageResourceServiceReference1;
using PheonixRt.Mvvm.LocalGeometryResourceServiceReference1;

using PheonixRt.Mvvm.Services;
using PheonixRt.Mvvm.ViewModels;

namespace PheonixRt.Mvvm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _imageSelectionManager =
                        new ImageSelectionManager(this.Dispatcher);

            // start up the response hosts
            ServiceHelper.StartResponseHosts();

            ICollectionView pgv = CollectionViewSource.GetDefaultView(_imageSelectionManager._patientGroups);
            pgv.CurrentChanged += pv_CurrentChanged;
            listViewPatients.ItemsSource = _imageSelectionManager._patientGroups; // pgv;

            listViewImages.ItemsSource = _imageSelectionManager._series;
            ICollectionView sgv = CollectionViewSource.GetDefaultView(_imageSelectionManager._series);
            sgv.CurrentChanged += cv_CurrentChanged;

            listViewStructures.ItemsSource = _imageSelectionManager._structures;

            var testDicomDataPath = Environment.GetEnvironmentVariable("TEST_DICOM_DATA");
            if (testDicomDataPath != null)
            {
                textDirectory.Text = testDicomDataPath;
            }
        }

        DicomImportPreprocessCoordinator _dipCoordinator =
            new DicomImportPreprocessCoordinator();

        ImageSelectionManager _imageSelectionManager;

        void cv_CurrentChanged(object sender, EventArgs e)
        {
            transverse.DataContext = null;
            coronal.DataContext = null;
            sagittal.DataContext = null;

            ICollectionView svg = CollectionViewSource.GetDefaultView(_imageSelectionManager._series);
            var seriesVm = (ImageSeriesViewModel)svg.CurrentItem;
            if (seriesVm == null)
                return;

            LocalImageResourceManagerClient cmsc1 =
                new LocalImageResourceManagerClient();

            var ivdc = cmsc1.GetImageVolumeBySeriesInstanceUID(seriesVm.SeriesInstanceUID);
            if (ivdc == null)
                return;

            cmsc1.Close();

            transverse.DataContext = new MprImageViewModel()
            {
                Orientation = MprGenerationContracts.Orientation.Transverse,
                ImageVolume = ivdc,
            };

            coronal.DataContext = new MprImageViewModel()
            {
                Orientation = MprGenerationContracts.Orientation.Coronal,
                ImageVolume = ivdc,
            };

            sagittal.DataContext = new MprImageViewModel()
            {
                Orientation = MprGenerationContracts.Orientation.Sagittal,
                ImageVolume = ivdc,
            };
        }

        void pv_CurrentChanged(object sender, EventArgs e)
        {
            ICollectionView pv = CollectionViewSource.GetDefaultView(_imageSelectionManager._patientGroups);
            if (pv.CurrentItem == null)
                return;

            var patientGroupItem = (PatientGroupViewModel)pv.CurrentItem;

            _imageSelectionManager._series.Clear();
            _imageSelectionManager._structures.Clear();

#if UPDATE_FROM_IMAGES
            UpdateFromImages(patientGroupItem);
#endif
            UpdateFromSeries(patientGroupItem);
            UpdateStructures(patientGroupItem);
        }

        private void UpdateFromImages(PatientGroupViewModel patientGroupItem)
        {
            LocalImageResourceManagerClient cmsc1 =
                new LocalImageResourceManagerClient();

            var idcs = from guid in cmsc1.GetImageResourceIds(patientGroupItem.PatientId)
                       select cmsc1.GetImage(guid);
            var idcsList = idcs.Where(thisIdc => thisIdc != null).ToList();

            foreach (var idc in idcsList)
            {
                int volumeVoxels = 0;

                Dispatcher.Invoke(() =>
                    ImageSelectionManager.AddOrUpdate<ImageSeriesViewModel>(
                        _imageSelectionManager._series,
                            s => s.SeriesInstanceUID.CompareTo(idc.SeriesInstanceUID) == 0,
                            s =>
                            {
                                s.InstanceCount++;
                                s.ResampleStatus =
                                    (volumeVoxels > 0)
                                        ? string.Format("Resampled ({0} voxels)", volumeVoxels)
                                        : "<not resampled>";
                            },
                            () =>
                            {
                                var isvm = ImageSeriesViewModel.Create(idc);
                                isvm.ResampleStatus = (volumeVoxels > 0)
                                    ? string.Format("Resampled ({0} voxels)", volumeVoxels)
                                    : "<not resampled>";
                                return isvm;
                            }));
            }

            cmsc1.Close();      
        }

        private void UpdateFromSeries(PatientGroupViewModel patientGroupItem)
        {
            LocalImageResourceManagerClient cmsc1 =
                new LocalImageResourceManagerClient();

            var ivdcs = from guid in cmsc1.GetImageVolumeResourceIds(patientGroupItem.PatientId)
                        select cmsc1.GetImageVolume(guid);

            // force generation
            var ivdcsList = ivdcs.ToList();

            foreach (var ivdc in ivdcsList)
            {
                Dispatcher.Invoke(() =>
                    ImageSelectionManager.AddOrUpdate<ImageSeriesViewModel>(
                        _imageSelectionManager._series,
                            s => s.SeriesInstanceUID.CompareTo(ivdc.Identity.SeriesInstanceUID) == 0,
                            s =>
                            {
                                s.ResampleStatus =
                                    (ivdc != null)
                                        ? string.Format("Resampled ({0} slices)", ivdc.Depth)
                                        : "<not resampled>";
                            },
                            () =>
                            {
                                var isvm = ImageSeriesViewModel.Create(ivdc);
                                isvm.ResampleStatus = (ivdc != null)
                                    ? string.Format("Resampled ({0} slices)", ivdc.Depth)
                                    : "<not resampled>";
                                return isvm;
                            }));
            }

            cmsc1.Close();
        }

        private void UpdateStructures(PatientGroupViewModel patientGroupItem)
        {
            LocalGeometryResourceManagerClient gmsc1 =
                new LocalGeometryResourceManagerClient();

            var sdcs = from guid in gmsc1.GetStructureResourceIds(patientGroupItem.PatientId)
                       select gmsc1.GetStructure(guid);
            int countSeries = sdcs.Count();

            foreach (var sdc in sdcs)
            {
                var smdc = gmsc1.GetSurfaceMeshByRelatedStructureId(sdc.Id);
                int meshVertices = smdc != null ? (int)smdc.VertexBuffer.ElementCount : 0;

                Dispatcher.Invoke(() =>
                ImageSelectionManager.AddOrUpdate<StructureViewModel>(
                            _imageSelectionManager._structures,
                                s => s.ROIName.CompareTo(sdc.ROIName) == 0,
                                s =>
                                {
                                    s.ROICount++;
                                    s.MeshStatus =
                                        (meshVertices > 0)
                                            ? string.Format("Meshed ({0} vertices)", meshVertices)
                                            : "<not meshed>";
                                },
                                () => new StructureViewModel(sdc.Id, sdc.ROIName)
                                {
                                    FrameOfReferenceUID = sdc.FrameOfReferenceUID,
                                    MeshStatus =
                                        (meshVertices > 0)
                                            ? string.Format("Meshed ({0} vertices)", meshVertices)
                                            : "<not meshed>"
                                }));
            }

            gmsc1.Close();
        }

        private void Button_Click_Scan(object sender, RoutedEventArgs e)
        {
            DicomImportPreprocessCoordinator.StartScan(textDirectory.Text);
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            //ICollectionView cv = CollectionViewSource.GetDefaultView(_images);
            //cv.MoveCurrentTo(null);

            //Task.Run(() =>
            //    {
            //        CacheManagerServiceReference1.CacheManagerServiceClient cmsc1 =
            //            new CacheManagerServiceReference1.CacheManagerServiceClient();

            //        string[] patientIds = cmsc1.GetPatientIds();
            //        foreach (string patientId in patientIds)
            //        { 
            //            var structureGuids = cmsc1.GetStructureResourceIds(patientId);
            //            foreach (var structureGuid in structureGuids)
            //            {
            //                cmsc1.RemoveStructure(structureGuid);
            //            }
            //        }

            //        foreach (var idc in _images.ToList())
            //        {
            //            cmsc1.RemoveImage(idc.Id);
            //            Dispatcher.Invoke(() => _images.Remove(idc));
            //        }

            //        cmsc1.Close();
            //    });
        }

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            // TODO: implement refresh button
            // pv_CurrentChanged(null, null);

            _imageSelectionManager.RefreshAll();
        }
    }
}
