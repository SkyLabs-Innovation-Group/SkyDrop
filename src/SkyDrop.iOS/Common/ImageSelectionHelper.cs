using System;
using System.IO;
using System.Threading.Tasks;
using AssetsLibrary;
using FFImageLoading.Extensions;
using Foundation;
using GMImagePicker;
using MvvmCross;
using Photos;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using UIKit;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Common
{
    public class ImageSelectionHelper
    {
        public static async void SelectMultiplePhoto(Action<string> successAction, Action failAction)
        {
            var hasPermission = await CheckPhotoPermission();
            if (!hasPermission)
                return;

            var picker = GetPicker();
            picker.Canceled += (s, e) => failAction();
            picker.FinishedPickingAssets += async (s, e) => await HandlePickedImages(e, successAction, failAction);

            var vc = Platform.GetCurrentUIViewController();
            vc.PresentModalViewController(picker, true);
        }

        private static async Task HandlePickedImages(MultiAssetEventArgs e, Action<string> successAction,
            Action failAction)
        {
            foreach (var phasset in e.Assets)
            {
                var filePath = await GetPathForImage(phasset);
                if (filePath == null)
                    failAction();

                successAction(filePath);
            }
        }

        private static Task<string> GetPathForImage(PHAsset asset)
        {
            var manager = new PHCachingImageManager();
            var tcs = new TaskCompletionSource<string>();
            var image = manager.RequestImageData(
                asset,
                new PHImageRequestOptions
                {
                    ResizeMode = PHImageRequestOptionsResizeMode.None,
                    DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat, NetworkAccessAllowed = true,
                    Synchronous = true
                },
                (data, dataUti, orientation, info) =>
                {
                    asset.RequestContentEditingInput(new PHContentEditingInputRequestOptions(), async (s, e) =>
                    {
                        var fileName = dataUti.ToString();
                        var path = await SaveImageFile(data, fileName);
                        tcs.TrySetResult(path);
                    });
                });

            return tcs.Task;
        }

        private static async Task<string> SaveImageFile(NSData imageData, string fileName)
        {
            try
            {
                var fileSystemService = Mvx.IoCProvider.Resolve<IFileSystemService>();
                using (var stream = imageData.AsStream())
                {
                    var extension = Path.GetExtension(fileName);
                    var newFilePath = Path.Combine(fileSystemService.CacheFolderPath, Guid.NewGuid() + extension);

                    using (var fileStream = File.Create(newFilePath))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.CopyToAsync(fileStream);
                    }

                    return newFilePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static async Task<string> UIImageToFile(UIImage image)
        {
            try
            {
                if (image == null)
                    return null;

                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Guid.NewGuid() + ".png");
                using (image)
                using (var stream = image.AsJpegStream())
                {
                    using (var fileStream = File.Create(path))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.CopyToAsync(fileStream);
                    }
                }

                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static GMImagePickerController GetPicker()
        {
            return new GMImagePickerController
            {
                Title = "Select Images",
                ColsInPortrait = 3,
                ColsInLandscape = 5,
                MinimumInteritemSpacing = 2.0f,
                ShowCameraButton = false,
                AutoSelectCameraImages = false,
                MediaTypes = new[] { PHAssetMediaType.Image },

                NavigationBarBackgroundColor = UIColor.White,
                NavigationBarTextColor = UIColor.Black,
                NavigationBarTintColor = UIColor.Black
            };
        }

        private static async Task<bool> CheckPhotoPermission()
        {
            var status = ALAssetsLibrary.AuthorizationStatus;
            if (status == ALAuthorizationStatus.Denied)
            {
                var alert = UIAlertController.Create(Strings.NoPhotoAccessTitle, Strings.NoPhotoAccessMessage,
                    UIAlertControllerStyle.Alert);

                alert.AddAction(UIAlertAction.Create(Strings.Cancel, UIAlertActionStyle.Cancel, null));
                alert.AddAction(UIAlertAction.Create(Strings.Settings, UIAlertActionStyle.Default,
                    action => UIApplication.SharedApplication.OpenUrl(
                        NSUrl.FromString(UIApplication.OpenSettingsUrlString))));

                var vc = Platform.GetCurrentUIViewController();
                await vc.PresentViewControllerAsync(alert, true);

                return false;
            }

            return true;
        }
    }
}