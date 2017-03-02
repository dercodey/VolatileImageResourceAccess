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

            // start up the response hosts
            ServiceHelper.StartResponseHosts();

            ICollectionView pgv = CollectionViewSource.GetDefaultView(_imageDisplayManager._patientGroups);
            pgv.CurrentChanged += pv_CurrentChanged;
            listViewPatients.ItemsSource = _imageDisplayManager._patientGroups; // pgv;

            listViewImages.ItemsSource = _imageDisplayManager._series;
            ICollectionView sgv = CollectionViewSource.GetDefaultView(_imageDisplayManager._series);
            sgv.CurrentChanged += cv_CurrentChanged;

            DicomLoaderManagerHelper.ImageStoredEvent += ImageStoredResponse_ImageStoredEvent;
            listViewStructures.ItemsSource = _imageDisplayManager._structures;

            var testDicomDataPath = Environment.GetEnvironmentVariable("TEST_DICOM_DATA");
            if (testDicomDataPath != null)
            {
                textDirectory.Text = testDicomDataPath;
            }
        }

        void ImageStoredResponse_ImageStoredEvent(string arg1, Guid imageGuid, double repoGb)
        {
            this.textRepositorySize.Text =
                string.Format("{0,4:F} GB", repoGb);
        }

        DicomImportPreprocessCoordinator _dipCoordinator =
            new DicomImportPreprocessCoordinator();

        ImageSelectionManager _imageDisplayManager =
            new ImageSelectionManager();

        void cv_CurrentChanged(object sender, EventArgs e)
        {
            transverse.DataContext = null;
            coronal.DataContext = null;
            sagittal.DataContext = null;

            ICollectionView svg = CollectionViewSource.GetDefaultView(_imageDisplayManager._series);
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
                ImageVolume = ivdc,
                Orientation = MprGenerationContracts.Orientation.Transverse
            };

            coronal.DataContext = new MprImageViewModel()
            {
                ImageVolume = ivdc,
                Orientation = MprGenerationContracts.Orientation.Coronal
            };

            sagittal.DataContext = new MprImageViewModel()
            {
                ImageVolume = ivdc,
                Orientation = MprGenerationContracts.Orientation.Sagittal
            };
        }

        void pv_CurrentChanged(object sender, EventArgs e)
        {
            ICollectionView pv = CollectionViewSource.GetDefaultView(_imageDisplayManager._patientGroups);
            if (pv.CurrentItem == null)
                return;

            var patientGroupItem = (PatientGroupViewModel)pv.CurrentItem;

            _imageDisplayManager._series.Clear();
            _imageDisplayManager._structures.Clear();

            Task.Run(() =>
                {
                    LocalImageResourceManagerClient cmsc1 =
                        new LocalImageResourceManagerClient();

                    cmsc1.ClearPrefetchStack();

                    var idcs = from guid in cmsc1.GetImageResourceIds(patientGroupItem.PatientId)
                                select cmsc1.GetImage(guid);
                    int count = idcs.Count();

                    foreach (var idc in 
                        idcs.Where(thisIdc => thisIdc != null))
                    {
                        var ivdc = cmsc1.GetImageVolumeBySeriesInstanceUID(idc.SeriesInstanceUID);
                        int volumeVoxels = 0;
                        if (ivdc != null)
                        {
                            volumeVoxels = (int)ivdc.PixelBuffer.ElementCount;
                            //cmsc1.PrefetchBuffer(ivdc.PixelBuffer);
                        }
 
                        Dispatcher.Invoke(() =>
                            ImageSelectionManager.AddOrUpdate<ImageSeriesViewModel>(
                                _imageDisplayManager._series,
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

                    var ivdcs = from guid in cmsc1.GetImageVolumeResourceIds(patientGroupItem.PatientId)
                               select cmsc1.GetImageVolume(guid);
                    count = ivdcs.Count();

                    foreach (var ivdc in ivdcs)
                    {
                        //int volumeVoxels = 0;
                        //if (ivdc != null)
                        //{
                        //    volumeVoxels = (int)ivdc.PixelBuffer.ElementCount;
                            
                        //    cmsc1.PrefetchBuffer(ivdc.PixelBuffer);
                        //}

                        Dispatcher.Invoke(() =>
                            ImageSelectionManager.AddOrUpdate<ImageSeriesViewModel>(
                                _imageDisplayManager._series,
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
                });

            Task.Run(() =>
                {
                    LocalGeometryResourceManagerClient cmsc1 =
                        new LocalGeometryResourceManagerClient();

                    var sdcs = from guid in cmsc1.GetStructureResourceIds(patientGroupItem.PatientId)
                                select cmsc1.GetStructure(guid);
                    int countSeries = sdcs.Count();

                    foreach (var sdc in sdcs)
                    {
                        var smdc = cmsc1.GetSurfaceMeshByRelatedStructureId(sdc.Id);
                        int meshVertices = smdc != null ? (int)smdc.VertexBuffer.ElementCount : 0;

                        Dispatcher.Invoke(() =>
                            ImageSelectionManager.AddOrUpdate<StructureViewModel>(
                                _imageDisplayManager._structures,
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

                    cmsc1.Close();
                });
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
    }
}
