using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

using System.Collections.Concurrent;
using System.ServiceModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Drawing;


using PheonixRt.DataContracts;

using PheonixRt.Mvvm.LocalImageResourceServiceReference1;
using PheonixRt.Mvvm.MprGenerationEngineServiceReference1;
using MprGenerationContracts;

namespace PheonixRt.Mvvm.ViewModels
{
    /// <summary>
    /// view model representing an MPR slice of a volume.
    /// Provides ImageVolumeData property ImageSource property to represent the current MPR
    /// </summary>
    public class MprImageViewModel : BindableBase
    {
        /// <summary>
        /// set up the view model to receive MPR generation events
        /// </summary>
        public MprImageViewModel()
        {
            if (_noImageImageSource == null)
            { 
                _noImageImageSource =
                    CreateBitmapSourceFromGdiBitmap(PheonixRt.Mvvm.Properties.Resources.NoImageImage);
            }

            if (_waitingImageSource == null)
            { 
                _waitingImageSource =
                    CreateBitmapSourceFromGdiBitmap(PheonixRt.Mvvm.Properties.Resources.WaitingImage);
            }

            ImageRenderManagerHelper.MprGenerationDoneEvent += MprGenerationDone_MprGenerationDoneEvent;
        }

        // stores the static bitmaps
        ImageSource _noImageImageSource;
        ImageSource _waitingImageSource;

        /// <summary>
        /// property for the current ImageVolume as source of the MPR
        /// </summary>
        public UniformImageVolumeDataContract ImageVolume
        {
            get { return _ivdc; }
            set
            {
                SetProperty(ref _ivdc, value);

                // reset the render transform
                ImageElementRenderTransform = new ScaleTransform();

                if (_ivdc == null)
                {
                    MprImageSource = _noImageImageSource;

                    // clear the bitmap
                    _bm = null;
                }
                else
                {
                    MprImageSource = _waitingImageSource;

                    // sets up the slice positions
                    SetupSlicePositions();

                    // queue up an update request
                    QueueRequest();
                }
            }
        }
        UniformImageVolumeDataContract _ivdc;

        /// <summary>
        /// represents the scale transform for the image element
        /// </summary>
        public ScaleTransform ImageElementRenderTransform
        {
            get { return _imageElementRenderTransform; }
            set { SetProperty(ref _imageElementRenderTransform, value); }
        }
        ScaleTransform _imageElementRenderTransform;

        /// <summary>
        /// represents the current MPR state
        /// </summary>
        public ImageSource MprImageSource
        {
            get { return _mprImageSource; }
            set { SetProperty(ref _mprImageSource, value); }
        }
        ImageSource _mprImageSource;

        /// <summary>
        /// string to indicate the lapsed time for MPR generation
        /// </summary>
        public string LapsedText
        {
            get { return _lapsedText; }
            set { SetProperty(ref _lapsedText, value); }
        }
        string _lapsedText;

        /// <summary>
        /// orientation of the MPR
        /// </summary>        
        public Orientation Orientation
        {
            get { return _orientation; }
            set 
            { 
                SetProperty(ref _orientation, value);
                SetupSlicePositions();

                QueueRequest();
            }
        }
        Orientation _orientation = Orientation.Transverse;

        /// <summary>
        /// slice position (as an integer) of the MPR
        /// </summary>
        public int SlicePosition
        {
            get { return _slicePosition; }
            set 
            { 
                SetProperty(ref _slicePosition, value);
                QueueRequest(); 
            }
        }
        int _slicePosition;

        /// <summary>
        /// returns the max slice position
        /// </summary>
        public int MaxSlicePosition
        {
            get { return _maxSlicePosition; }
            set
            {
                SetProperty(ref _maxSlicePosition, value);
            }
        }
        int _maxSlicePosition;

        /// <summary>
        /// sets up the slice position
        /// </summary>
        void SetupSlicePositions()
        {
            // set slider control
            switch (Orientation)
            {
                case Orientation.Transverse:
                    MaxSlicePosition = ImageVolume.Depth;
                    break;

                case Orientation.Coronal:
                    MaxSlicePosition = ImageVolume.Height;
                    break;

                case Orientation.Sagittal:
                    MaxSlicePosition = ImageVolume.Width;
                    break;
            }
            SlicePosition = MaxSlicePosition / 2;
        }

        /// <summary>
        /// window center for the generated MPR
        /// </summary>
        public int WindowCenter
        {
            get { return _windowCenter; }
            set 
            { 
                SetProperty(ref _windowCenter, value);
                QueueRequest();
            }
        }
        int _windowCenter = 150;

        /// <summary>
        /// window width for the generated MPR
        /// </summary>
        public int WindowWidth
        {
            get { return _windowWidth; }
            set 
            { 
                SetProperty(ref _windowWidth, value);
                QueueRequest();
            }
        }
        int _windowWidth = 200;

        /// <summary>
        /// on a generation event, update the relevent properties
        /// </summary>
        /// <param name="methodID">the invoking methodID</param>
        /// <param name="idc">the image data contract that was generated</param>
        /// <param name="requestTime">when was the MPR requested</param>
        void MprGenerationDone_MprGenerationDoneEvent(string methodID, ImageDataContract idc, DateTime requestTime)
        {
            // get the request from the queue
            MprGenerationRequestV1 request;
            if (_queueWaiting.TryRemove(new Guid(methodID), out request))
            {
                // make sure the request matches the volume
                System.Diagnostics.Trace.Assert(_ivdc.Identity.Guid.CompareTo(request.ImageVolumeId) == 0);

                // update the bitmap (must occur on the UI thread)
                Dispatcher.CurrentDispatcher.Invoke(() => UpdateImageSourceFromImageDataContract(idc, requestTime));

                // and remove the generated image
                LocalImageResourceManagerClient lirm =
                    new LocalImageResourceManagerClient();
                lirm.RemoveImage(idc.ImageId);
                lirm.Close();
            }
        }

        /// <summary>
        /// updates the image source for the given IDC return
        /// </summary>
        /// <param name="idc">the IDC representing the updated MPR</param>
        /// <param name="requestTime">the time of request, for calculating lapsed</param>
        void UpdateImageSourceFromImageDataContract(ImageDataContract idc, DateTime requestTime)
        {
            // now update with the result to the writeable bitmap
            var wb = GetWriteableBitmap();

            var handle = idc.PixelBuffer.GetHandle();
            wb.WritePixels(new Int32Rect(0, 0, _bm.PixelWidth, _bm.PixelHeight),
                handle.DangerousGetHandle(), (int)handle.ByteLength, _bm.PixelWidth * sizeof(ushort));
            idc.PixelBuffer.ReleaseHandle();

            // update the ImageSource (which should trigger re-binding)
            MprImageSource = wb;

            // called to update the scale for the image element (presumably bound to the image element)
            UpdateImageElementScale();
            
            // update the time lapsed text
            LapsedText = (DateTime.Now - requestTime).Milliseconds.ToString("0 msec");
        }

        /// <summary>
        /// allocates (if needed) and returns the writeable bitmap to be populated with the MPR content
        /// </summary>
        /// <returns>the allocated (and updated) writeable bitmap</returns>
        WriteableBitmap GetWriteableBitmap()
        {
            int desiredWidth = 0;
            int desiredHeight = 0;
            switch (Orientation)
            {
                case Orientation.Transverse:
                    desiredWidth = _ivdc.Width;
                    desiredHeight = _ivdc.Height;
                    break;

                case Orientation.Coronal:
                    desiredWidth = _ivdc.Width;
                    desiredHeight = _ivdc.Depth;
                    break;

                case Orientation.Sagittal:
                    desiredWidth = _ivdc.Height;
                    desiredHeight = _ivdc.Depth;
                    break;
            }

            if (_bm == null
                || _bm.PixelWidth != desiredWidth
                || _bm.PixelHeight != desiredHeight)
            {
                _bm = new WriteableBitmap(desiredWidth, desiredHeight, 75, 75, PixelFormats.Gray16, null);
            }

            return _bm;
        }
        WriteableBitmap _bm;

        /// <summary>
        /// updates the ImageElementScale transform, based on the dimensions of the image volume
        /// </summary>
        void UpdateImageElementScale()
        {
            switch (Orientation)
            {
                case Orientation.Transverse:
                    break;

                case Orientation.Coronal:
                    ImageElementRenderTransform = new ScaleTransform()
                    {
                        ScaleY = _ivdc.VoxelSpacing.Z
                            / _ivdc.VoxelSpacing.X
                    };
                    break;

                case Orientation.Sagittal:
                    ImageElementRenderTransform = new ScaleTransform()
                    {
                        ScaleY = _ivdc.VoxelSpacing.Z
                            / _ivdc.VoxelSpacing.Y
                    };
                    break;
            }
        }

        /// <summary>
        /// queues the request to update the MPR
        /// </summary>
        void QueueRequest()
        {
            if (_ivdc == null)
                return;

            MprGenerationRequestV1 request =
                new MprGenerationRequestV1();
            request.RequestTime = DateTime.Now;
            request.ImageVolumeId = _ivdc.Identity.Guid;
            request.SlicePosition = this.SlicePosition;
            request.Orientation = this.Orientation;
            request.WindowCenter = this.WindowCenter;
            request.WindowWidth = this.WindowWidth;

            if (_mges == null
                || _mges.State != CommunicationState.Opened)
            {
                _mges = new MprGenerationEngineServiceClient();
            }

            OperationContextScope contextScope = new OperationContextScope(_mges.InnerChannel);
            {
                Guid methodId = Guid.NewGuid();
                this._queueWaiting.TryAdd(methodId, request);

                ImageRenderManagerHelper.SetupResponseHeader(methodId);
                _mges.GenerateMpr(request);
            }

            // TODO: determine when to close
            // _mges.Close();
        }

        MprGenerationEngineServiceClient _mges;

        ConcurrentDictionary<Guid, MprGenerationRequestV1> _queueWaiting =
            new ConcurrentDictionary<Guid, MprGenerationRequestV1>();

        /// <summary>
        /// creates a BitmapSource from a GDI Bitmap
        /// </summary>
        /// <param name="bitmap">the GDI bitmap to be used</param>
        /// <returns>the update BitmapSource</returns>
        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
