using System;
using AVFoundation;
using CoreImage;
using CoreMedia;
using Foundation;
using GameKit;
using Photos;
using UIKit;

namespace Xamarin.iOS_BAC_.Views
{
    public partial class EditPictureViewController : UIViewController
    {
        private UIImagePickerController imagePickerForCamera;
        private AVCaptureSession captureSession;
        private UIImage originalImage;
        private AVCaptureStillImageOutput stillImageOutput;
        private bool clicked = false;
        public EditPictureViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            EditPictureButton.Enabled = false;
            SavePictureButton.Enabled = false;
        }

        private void GetPicture()
        {
            captureSession = new AVCaptureSession();
            var videoPreviewLayer = new AVCaptureVideoPreviewLayer(captureSession);
            videoPreviewLayer.Frame = this.View.Frame;
            this.View.Layer.AddSublayer(videoPreviewLayer);

            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            AVCaptureDevice captureDevice = null;

            foreach (var device in devices)
            {
                if (device.Position == AVCaptureDevicePosition.Front)
                {
                    captureDevice = device;
                }
            }

            // Configure Camera on device - check if some features are supported on the device
            ConfigureCameraForDevice(captureDevice);

            // Define Device Input and add it to the Session
            var captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
            captureSession.AddInput(captureDeviceInput);

            // NSDictionary settings
            var settings = new NSMutableDictionary();
            settings[AVVideo.CodecKey] = new NSNumber((int)AVVideoCodec.JPEG);

            // Define Still Image Output
            stillImageOutput = new AVCaptureStillImageOutput();

            // Add Output to Session ans Start
            captureSession.AddOutput(stillImageOutput);
            captureSession.StartRunning();
            /*
            imagePickerForCamera = new UIImagePickerController();
            imagePickerForCamera.SourceType = UIImagePickerControllerSourceType.Camera;

            imagePickerForCamera.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera);

            //imagePickerForCamera.FinishedPickingMedia += Handle_FinishedPickingMedia;
           // imagePickerForCamera.Canceled += Handle_Canceled; */
        }

        public void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        private async void CapturePhoto()
        {
            var videoConnection = stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
            CMSampleBuffer sampleBuffer = await stillImageOutput.CaptureStillImageTaskAsync(videoConnection);

            var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);
            originalImage = UIImage.LoadFromData(jpegImageAsNsData);

           // this.PerformSegue("SegueToWaitingScreen", this);
        }

        private void EditPicture()
        {
            if (clicked)
            {
                PictureView.Image = originalImage;
                clicked = false;
            }
            else
            {
                var uiimage = PictureView.Image;
                originalImage = uiimage;
                var image = new CIImage(uiimage.CGImage);

                var filter = new CIMinimumComponent()
                {
                    Image = image
                };

                var output = filter.OutputImage;

                var context = CIContext.FromOptions(null);
                var cgimage = context.CreateCGImage(output, output.Extent);

                PictureView.Image = UIImage.FromImage(cgimage);
                clicked = true;
            }
        }

        private void SavePicture()
        {
            var image = PictureView.Image;
            bool allowed = false;

            PHPhotoLibrary.RequestAuthorization(status =>
            {
                switch (status)
                {
                    case PHAuthorizationStatus.Denied:
                    case PHAuthorizationStatus.Restricted:

                        return;
                    case PHAuthorizationStatus.Authorized:
                        allowed = true;
                        break;
                }
            });
            if (allowed)
            {
                image.SaveToPhotosAlbum((UIimage, error) =>
                {
                    var o = UIimage as UIImage;
                    Console.WriteLine("error:"+error);
                });
            }
        }

        partial void TakePictureButton_TouchUpInside(UIButton sender)
        {
            //GetPicture();
            EditPictureButton.Enabled = true;
            SavePictureButton.Enabled = true;
        }

        partial void EditPictureButton_TouchUpInside(UIButton sender)
        {
            EditPicture();
        }

        partial void SavePictureButton_TouchUpInside(UIButton sender)
        {
            SavePicture();
        }
    }
}