using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using Dicom.Imaging.LUT;

using PheonixRt.DataContracts;

using DicomLoaderManager.LocalGeometryResourceServiceReference1;
using DicomLoaderManager.LocalImageResourceServiceReference1;
using NServiceBus;
using Contracts.DicomLoader;

namespace DicomLoaderManager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class DicomScanManager : IDicomScanManager, IHandleMessages<ScanDirectory>
    {
        static IEndpointInstance _endpointInstance = null;

        public static void InitializeEndpoint()
        {
            _endpointInstance = ConfigureSBEndpoint().GetAwaiter().GetResult();
        }

        private static async Task<IEndpointInstance> ConfigureSBEndpoint()
        {
            var endpointConfiguration = new EndpointConfiguration("DicomLoaderManager.DicomLoaderManager");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            return endpointInstance;
        }

        string _pathname;
        bool _rescan = false;

        public void ScanDirectory(string pathname, bool rescan)
        {
            _pathname = pathname;
            _rescan = rescan;

            Dicom.Media.DicomFileScanner dfs = new Dicom.Media.DicomFileScanner();
            dfs.FileFound += dfs_FileFound;
            dfs.Progress += dfs_Progress;
            dfs.Complete += dfs_Complete;
            dfs.Start(pathname);
        }

        // stores all the series' processed during an 'association'
        List<string> _seriesInstanceUIDs = new List<string>();

        void dfs_FileFound(Dicom.Media.DicomFileScanner scanner, Dicom.DicomFile file, string fileName)
        {
            DicomDataset ds = file.Dataset;

            string modality = ds.Get<string>(DicomTag.Modality);
            // Console.WriteLine(string.Format("Found {0} {1}", modality, ds.Get<DicomUID>(DicomTag.SOPInstanceUID)));

            if (modality == null)
            {
                // handle this?
            }
            else if (modality.CompareTo("RTSTRUCT") == 0)
            {
                StoreStructureSet(ds);
            }
            else if (modality.CompareTo("RTPLAN") == 0)
            {
                // process plan
                Console.WriteLine("RTPLAN");
            }
            else if (modality.CompareTo("REG") == 0)
            {
                // process registration
                Console.WriteLine("REG");
            }
            else if (modality.CompareTo("RTRECORD") == 0)
            {
                // process RT RECORD
                Console.WriteLine("RTRECORD");
            }
            else if (modality.CompareTo("SR") == 0)
            {
                // process Structured Report
                Console.WriteLine("SR");
            }
            else
            {
                StoreImage(ds, modality);
            }
        }

        void StoreImage(DicomDataset ds, string modality)
        {
            DicomImage di = new DicomImage(ds);

            // store in cached resource
            var idc = new ImageDataContract();
            idc.PatientId = ds.Get<string>(DicomTag.PatientID);

            if (ds.Contains(DicomTag.PixelSpacing))
            {
                idc.PixelSpacing = new VoxelSize()
                {
                    X = Convert.ToSingle(ds.Get<double>(DicomTag.PixelSpacing, 0)),
                    Y = Convert.ToSingle(ds.Get<double>(DicomTag.PixelSpacing, 1)),
                };
            }
            else
            {
                idc.PixelSpacing = new VoxelSize() 
                { 
                    X = 1.0f, 
                    Y = 1.0f, 
                };
            }

            idc.ImagePosition = new ImagePosition()
            {
                X = Convert.ToSingle(ds.Get<double>(DicomTag.ImagePositionPatient, 0)),
                Y = Convert.ToSingle(ds.Get<double>(DicomTag.ImagePositionPatient, 1)),
                Z = Convert.ToSingle(ds.Get<double>(DicomTag.ImagePositionPatient, 2)),
            };

            idc.ImageOrientation = new ImageOrientation();
            idc.ImageOrientation.Row = new DirectionCosine()
            {
                X = Convert.ToSingle(ds.Get<double>(DicomTag.ImageOrientationPatient, 0)),
                Y = Convert.ToSingle(ds.Get<double>(DicomTag.ImageOrientationPatient, 1)),
                Z = Convert.ToSingle(ds.Get<double>(DicomTag.ImageOrientationPatient, 2)),
            };

            idc.ImageOrientation.Column = new DirectionCosine()
            {
                X = Convert.ToSingle(ds.Get<double>(DicomTag.ImageOrientationPatient, 3)),
                Y = Convert.ToSingle(ds.Get<double>(DicomTag.ImageOrientationPatient, 4)),
                Z = Convert.ToSingle(ds.Get<double>(DicomTag.ImageOrientationPatient, 5)),
            };

            idc.Width = di.Width;
            idc.Height = di.Height;
            idc.Label = string.Format("{0} {1}",
                modality,
                ds.GetDateTime(DicomTag.SeriesDate, DicomTag.SeriesTime).ToString());
            idc.SeriesInstanceUID = ds.Get<string>(DicomTag.SeriesInstanceUID);

            // store for association closed event
            _seriesInstanceUIDs.Add(idc.SeriesInstanceUID);

            string for_uid = ds.Get<string>(DicomTag.FrameOfReferenceUID);
            idc.FrameOfReferenceUID = for_uid;

            LocalImageResourceManagerClient
                cache1 = new LocalImageResourceManagerClient();

            idc = cache1.AddImage(idc);
            double repoGb = cache1.GetRepositorySizeGB();

            cache1.Close();

            if (di.PhotometricInterpretation == PhotometricInterpretation.Monochrome1
                || di.PhotometricInterpretation == PhotometricInterpretation.Monochrome2)
            {
                var dsForWl = di.Dataset;
                if (_firstImageIn.ContainsKey(idc.SeriesInstanceUID))
                {
                    dsForWl = _firstImageIn[idc.SeriesInstanceUID].Dataset;
                }
                else
                {
                    _firstImageIn.TryAdd(idc.SeriesInstanceUID, di);
                }

                var gro = GrayscaleRenderOptions.FromDataset(dsForWl);
                var voilut = VOILUT.Create(gro);

                var ipd = PixelDataFactory.Create(di.PixelData, 0);

                int[] outPixelsInt = new int[di.Width * di.Height];
                ipd.Render(voilut, outPixelsInt);

                ushort[] outPixelsUshort = Array.ConvertAll(outPixelsInt,
                    new Converter<int, ushort>(inInt => (ushort)(inInt)));
                var handle = idc.PixelBuffer.GetHandle();
                handle.WriteArray<ushort>(0, outPixelsUshort, 0, outPixelsUshort.Length);
                idc.PixelBuffer.ReleaseHandle();
                idc.PixelBuffer.CloseMapping();

                // publish the image stored message
                _endpointInstance.Publish(new ImageStored
                {
                    ImageGuid = idc.ImageId,
                    RepoGb = repoGb,
                });
            }
        }

        ConcurrentDictionary<string, DicomImage> _firstImageIn =
            new ConcurrentDictionary<string, DicomImage>();

        void StoreStructureSet(DicomDataset ds)
        {
            // store in cached resource
            LocalGeometryResourceManagerClient
                cache1 = new LocalGeometryResourceManagerClient();

            string seriesInstanceUID = ds.Get<string>(DicomTag.SeriesInstanceUID);
            string sopInstanceUID = ds.Get<string>(DicomTag.SOPInstanceUID);
            string structureSetLabel = ds.Get<string>(DicomTag.StructureSetLabel);

            DicomSequence referencedForSequence = ds.Get<DicomSequence>(DicomTag.ReferencedFrameOfReferenceSequence);
            string frameOfReferenceUID = referencedForSequence.First().Get<string>(DicomTag.FrameOfReferenceUID);

            DicomSequence ssRoiSequence = ds.Get<DicomSequence>(DicomTag.StructureSetROISequence);
            foreach (var ssRoiItem in ssRoiSequence)
            {
                // process structure set
                StructureDataContract sdc = new StructureDataContract();
                sdc.PatientId = ds.Get<string>(DicomTag.PatientID);
                sdc.SeriesInstanceUID = seriesInstanceUID;
                sdc.SOPInstanceUID = sopInstanceUID;
                sdc.StructureSetLabel = structureSetLabel;
                string roiName = ssRoiItem.Get<string>(DicomTag.ROIName);
                sdc.ROIName = roiName;
                int roiNumber = ssRoiItem.Get<int>(DicomTag.ROINumber);
                // sdc.ROINumber = roiNumber;

                string relatedForUID = ssRoiItem.Get<string>(DicomTag.ReferencedFrameOfReferenceUID);
                sdc.FrameOfReferenceUID = relatedForUID;

                var roiContourSeq = ds.Get<DicomSequence>(DicomTag.ROIContourSequence);
                var roiContourItem = from item in roiContourSeq
                                     where item.Get<int>(DicomTag.ReferencedROINumber) == roiNumber
                                     select item;
                System.Diagnostics.Trace.Assert(roiContourItem.Count() == 1);

                List<Guid> polygonGuidList = new List<Guid>();
                var contourSequence = roiContourItem.First().Get<DicomSequence>(DicomTag.ContourSequence);
                if (contourSequence != null)
                {
                    foreach (var contourItem in contourSequence)
                    {
                        var contourGeometricType = contourItem.Get<string>(DicomTag.ContourGeometricType);
                        // TODO: if it is a point then store differently

                        var numberOfContourPoints = contourItem.Get<int>(DicomTag.NumberOfContourPoints);
                        ContourDataContract pdc = new ContourDataContract();
                        pdc.FrameOfReferenceUID = sdc.FrameOfReferenceUID;
                        pdc.VertexCount = numberOfContourPoints;
                        pdc = cache1.AddPolygon(pdc);
                        polygonGuidList.Add(pdc.Id);

                        // now access vertex buffer

                        var handle = pdc.VertexBuffer.GetHandle();
                        for (int n = 0; n < numberOfContourPoints; n++)
                        {
                            var x = contourItem.Get<double>(DicomTag.ContourData, n * 3 + 0);
                            var y = contourItem.Get<double>(DicomTag.ContourData, n * 3 + 1);
                            var z = contourItem.Get<double>(DicomTag.ContourData, n * 3 + 2);
                            Vector3D vertex = new Vector3D(x, y, z);

                            handle.Write<Vector3D>((ulong)(n * Marshal.SizeOf(vertex)), vertex);
                        }
                        pdc.VertexBuffer.ReleaseHandle();
                    }
                }

                sdc.Contours = polygonGuidList;
                sdc = cache1.AddStructure(sdc);

                // publish the structure stored message
                _endpointInstance.Publish(new StructureStored
                {
                    StructureGuid = sdc.Id,
                });
            }

            //proxy.Close();

            cache1.Close();

        }

        string _currentDirectory = string.Empty;

        void dfs_Progress(Dicom.Media.DicomFileScanner scanner, string directory, int count)
        {
            // only process if initial directory change
            if (directory.CompareTo(_currentDirectory) == 0)
                return;

            _currentDirectory = directory;

            // only process if there are series'
            if (_seriesInstanceUIDs.Count() == 0)
                return;


            _endpointInstance.Publish(new AssociationClosed()
            {
                SeriesInstanceUids = _seriesInstanceUIDs.Distinct().ToArray(),
            });

            _seriesInstanceUIDs.Clear();
        }

        void dfs_Complete(Dicom.Media.DicomFileScanner scanner)
        {
            scanner.FileFound -= dfs_FileFound;
            scanner.Complete -= dfs_Complete;

            _endpointInstance.Publish(new AssociationClosed()
            {
                SeriesInstanceUids = _seriesInstanceUIDs.Distinct().ToArray(),
            });

            _seriesInstanceUIDs.Clear();

            // don't currently support rescanning
            System.Diagnostics.Trace.Assert(!_rescan);
        }

        public Task Handle(ScanDirectory message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
