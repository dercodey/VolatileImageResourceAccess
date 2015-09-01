using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Collections.Concurrent;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.ServiceModel.Channels;

using ServiceModelEx;

using PheonixRt.DataContracts;

using MprGenerationEngine.LocalImageResourceServiceReference1;
using MprGenerationContracts;

namespace MprGenerationEngine
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class MprGenerationEngineService : IMprGenerationEngineService
    {
        public void GenerateMpr(MprGenerationRequestV1 request)
        {
            var responseContext =
                OperationContext.Current.IncomingMessageHeaders.GetHeader<ResponseContext>(
                    "ResponseContext", "ServiceModelEx");
            System.Diagnostics.Trace.Assert(responseContext.MethodId.CompareTo(Guid.Empty.ToString()) != 0);

            LocalImageResourceManagerClient lirm =
                new LocalImageResourceManagerClient();
            lirm.Open();

            UniformImageVolumeDataContract ivdc = null;
            ivdc = lirm.GetImageVolume(request.ImageVolumeId);

            int[] size = CalculateSize(ivdc, request.Orientation);

            ImageDataContract idc = null;
            _cacheResultImages.TryGetValue(responseContext.MethodId, out idc);
            if (idc == null
                || idc.Width != size[0]
                || idc.Height != size[1])
            {
                idc = new ImageDataContract();
                idc.Width = size[0];
                idc.Height = size[1];
                idc = lirm.AddImage(idc);
                _cacheResultImages.TryAdd(responseContext.MethodId, idc);
            }

            lirm.Close();

            UpdatePixelsFromVolumeResampled(ivdc,
                request.Orientation, request.SlicePosition,
                request.WindowCenter, request.WindowWidth,
                idc);

            MessageHeader<ResponseContext> responseHeader = new MessageHeader<ResponseContext>(responseContext);
            NetMsmqBinding binding = new NetMsmqBinding("NoMsmqSecurity");
            MprGenerationResponseProxy proxy = new MprGenerationResponseProxy(binding,
                new EndpointAddress(responseContext.ResponseAddress));

            using (OperationContextScope scope = new OperationContextScope(proxy.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(
                    responseHeader.GetUntypedHeader("ResponseContext", "ServiceModelEx"));

                proxy.OnMprDone(idc, request.RequestTime);
            }

            proxy.Close();
        }

        ConcurrentDictionary<string, ImageDataContract> _cacheResultImages =
            new ConcurrentDictionary<string, ImageDataContract>();

        int[] CalculateSize(UniformImageVolumeDataContract ivdc, Orientation orientation)
        {
            int[] size = new int[2];
            switch (orientation)
            {
                case Orientation.Transverse:
                    size[0] = ivdc.Width;
                    size[1] = ivdc.Height;
                    break;

                case Orientation.Coronal:
                    size[0] = ivdc.Width;
                    size[1] = ivdc.Depth;
                    break;

                case Orientation.Sagittal:
                    size[0] = ivdc.Height;
                    size[1] = ivdc.Depth;
                    break;
            }
            return size;
        }

        void UpdatePixelsFromVolumeResampled(UniformImageVolumeDataContract ivdc, 
            Orientation orientation, int slice, 
            int windowCenter, int windowWidth, 
            ImageDataContract idc)
        {
            var inHandle = ivdc.PixelBuffer.GetHandle();
            var outHandle = idc.PixelBuffer.GetHandle();
            for (int nAtRow = 0; nAtRow < idc.Height; nAtRow++)
            {
                for (int nAtCol = 0; nAtCol < idc.Width; nAtCol++)
                {
                    int srcOffset = 0;
                    switch (orientation)
                    {
                        case Orientation.Transverse:
                            srcOffset = slice;
                            srcOffset *= ivdc.Height;
                            srcOffset += nAtRow;
                            srcOffset *= ivdc.Width;
                            srcOffset += nAtCol;
                            break;

                        case Orientation.Coronal:
                            srcOffset = nAtRow;
                            srcOffset *= ivdc.Height;
                            srcOffset += slice;
                            srcOffset *= ivdc.Width;
                            srcOffset += nAtCol;
                            break;

                        case Orientation.Sagittal:
                            srcOffset = nAtRow;
                            srcOffset *= ivdc.Height;
                            srcOffset += nAtCol;
                            srcOffset *= ivdc.Width;
                            srcOffset += slice;
                            break;
                    }

                    ulong byteOffset = (ulong)srcOffset * sizeof(ushort);
                    if (byteOffset < inHandle.ByteLength)
                    {
                        int dstOffset = nAtRow;
                        dstOffset *= idc.Width;
                        dstOffset += nAtCol;

                        int val = inHandle.Read<ushort>(byteOffset);
                        val = (val - windowCenter) * (int)ushort.MaxValue/2 
                            / windowWidth + (int)ushort.MaxValue/4;
                        val *= 2;
                        val = Math.Max(0, val);
                        val = Math.Min(ushort.MaxValue, val);
                        outHandle.Write<ushort>((ulong)(dstOffset*sizeof(ushort)), (ushort)val);
                    }
                }
            }
            idc.PixelBuffer.ReleaseHandle();
            ivdc.PixelBuffer.ReleaseHandle();
        }

        byte[] CreateStreamFromVolumeResampled(UniformImageVolumeDataContract ivdc,
            Orientation orientation, int slice,
            int windowCenter, int windowWidth, int[] size)
        {
            var inHandle = ivdc.PixelBuffer.GetHandle();
            ushort[] outPixels = new ushort[size[0]*size[1]];
            for (int nAtRow = 0; nAtRow < size[1]; nAtRow++)
            {
                for (int nAtCol = 0; nAtCol < size[0]; nAtCol++)
                {
                    int srcOffset = 0;
                    switch (orientation)
                    {
                        case Orientation.Transverse:
                            srcOffset = slice;
                            srcOffset *= ivdc.Height;
                            srcOffset += nAtRow;
                            srcOffset *= ivdc.Width;
                            srcOffset += nAtCol;
                            break;

                        case Orientation.Coronal:
                            srcOffset = nAtRow;
                            srcOffset *= ivdc.Height;
                            srcOffset += slice;
                            srcOffset *= ivdc.Width;
                            srcOffset += nAtCol;
                            break;

                        case Orientation.Sagittal:
                            srcOffset = nAtRow;
                            srcOffset *= ivdc.Height;
                            srcOffset += nAtCol;
                            srcOffset *= ivdc.Width;
                            srcOffset += slice;
                            break;
                    }

                    ulong byteOffset = (ulong)srcOffset * sizeof(ushort);
                    if (byteOffset < inHandle.ByteLength)
                    {
                        int dstOffset = nAtRow;
                        dstOffset *= size[0];
                        dstOffset += nAtCol;

                        int val = inHandle.Read<ushort>(byteOffset);
                        val = (val - windowCenter) * (int)ushort.MaxValue/2
                            / windowWidth + (int)ushort.MaxValue / 4;
                        val *= 2;
                        val = Math.Max(0, val);
                        val = Math.Min(ushort.MaxValue, val);
                        outPixels[dstOffset] = (ushort)val;
                    }
                }
            }
            ivdc.PixelBuffer.ReleaseHandle();
            BitmapSource outBitmap = BitmapSource.Create(size[0], size[1], 96.0, 96.0, 
                PixelFormats.Gray16, null, outPixels, size[0]*sizeof(ushort));
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(outBitmap));

            var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream.GetBuffer();
        }

    }
}
