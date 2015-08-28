using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using PheonixRt.DataContracts;
using PheonixRt.ResampleServiceContracts;
using PheonixRt.ResampleEngineService.LocalImageResourceServiceReference1;

namespace PheonixRt.ResampleEngineService
{
    /// <summary>
    /// 
    /// </summary>
    public class ResampleEngine : IResampleEngine
    {
        /// <summary>
        /// TODO: return response, not contract
        /// </summary>
        /// <param name="seriesInstanceUID"></param>
        /// <returns></returns>        
        public ImageVolumeResampleResponse ResampleImageVolume(ImageVolumeResampleRequest request)
        {
            LocalImageResourceManagerClient cmsc1 =
                new LocalImageResourceManagerClient();

            var imageIds = cmsc1.GetImageIdsBySeries(request.SeriesInstanceUID);
            if (imageIds.Count() == 0)
                return null;

            var imagesInSeries = (from guid in imageIds 
                                  select cmsc1.GetImage(guid)
                                  ).OrderBy(idc => -idc.ImagePosition.Z);

            var ivdc = new UniformImageVolumeDataContract();
            ivdc.Identity.SeriesInstanceUID = request.SeriesInstanceUID;
            ivdc.Identity.Label = imagesInSeries.First().Label;
            ivdc.Identity.PatientId = imagesInSeries.First().PatientId;
            ivdc.FrameOfReferenceUID = imagesInSeries.First().FrameOfReferenceUID;

            ivdc.Width = imagesInSeries.First().Width;
            ivdc.Height = imagesInSeries.First().Height;
            ivdc.Depth = imagesInSeries.Count();

            var imageOrientation = imagesInSeries.First().ImageOrientation;
            ivdc.VolumeOrientation = new VolumeOrientation()
            {
                Row = imageOrientation.Row,
                Column = imageOrientation.Column,
            };

            var voxelSpacing = new VoxelSize()
             {
                 X = imagesInSeries.First().PixelSpacing.X,
                 Y = imagesInSeries.First().PixelSpacing.Y,
                 Z = Math.Abs(imagesInSeries.First().ImagePosition.Z
                     - imagesInSeries.Last().ImagePosition.Z) / imagesInSeries.Count()
             };
            ivdc.VoxelSpacing = voxelSpacing;

            // take cross product
            // ivdc.DirectionZ = 

            ivdc = cmsc1.AddImageVolume(ivdc);

            var outVoxelHandle = ivdc.PixelBuffer.GetHandle();
            ushort[] frame = new ushort[imagesInSeries.First().PixelBuffer.ElementCount];
            ulong currentVoxelOffset = 0;
            foreach (var idc in imagesInSeries)
            {
                // get access to the buffer
                var inPixelHandle = idc.PixelBuffer.GetHandle();
                inPixelHandle.ReadArray<ushort>(0, frame, 0, frame.Length);
                outVoxelHandle.WriteArray<ushort>(currentVoxelOffset,
                    frame, 0, frame.Length);
                idc.PixelBuffer.ReleaseHandle();
                idc.PixelBuffer.CloseMapping();

                currentVoxelOffset += (ulong)(idc.PixelBuffer.ElementCount * sizeof(ushort));
            }

            ivdc.PixelBuffer.ReleaseHandle();
            ivdc.PixelBuffer.CloseMapping();

            cmsc1.Close();

            System.Diagnostics.Trace.Assert(ivdc != null);
            System.Diagnostics.Trace.Assert(ivdc.Identity != null);
            System.Diagnostics.Trace.Assert(ivdc.Identity.Guid.CompareTo(Guid.Empty) != 0);
            return new ImageVolumeResampleResponse()
            {
                SeriesInstanceUID = request.SeriesInstanceUID,
                ResampledImageVolumeGuid = ivdc.Identity.Guid
            };
        }
    }
}
