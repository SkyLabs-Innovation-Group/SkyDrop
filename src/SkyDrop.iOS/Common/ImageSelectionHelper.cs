using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AssetsLibrary;
using CoreGraphics;
using FFImageLoading.Extensions;
using Foundation;
using GMImagePicker;
using Photos;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Common
{
	public class ImageSelectionHelper
	{
        public static async void SelectMultiplePhoto(DropViewModel dropViewModel)
        {
            var picker = new GMImagePickerController();

            picker.Title = "Select Images";
            picker.ColsInPortrait = 3;
            picker.ColsInLandscape = 5;
            picker.MinimumInteritemSpacing = 2.0f;
            picker.ShowCameraButton = false;
            picker.AutoSelectCameraImages = false;
            picker.MediaTypes = new[] { PHAssetMediaType.Image };

            picker.NavigationBarBackgroundColor = UIColor.White;
            picker.NavigationBarTextColor = UIColor.Black;
            picker.NavigationBarTintColor = UIColor.Black;

            var hasPermission = await CheckPhotoPermissionAsync();
            if (!hasPermission)
                return;

            picker.Canceled += (s, e) => dropViewModel.IosMultipleImageSelectTask.TrySetResult(false);
            picker.FinishedPickingAssets +=
            async (sender, args) =>
            {
                foreach (var phasset in args.Assets)
                {
                    PHCachingImageManager manager = new PHCachingImageManager();
                    var tcs = new TaskCompletionSource<bool>();
                    var resultFilePaths = new List<string>();
                    var image = manager.RequestImageForAsset(
                        asset: phasset,
                        targetSize: new CGSize(phasset.PixelWidth, phasset.PixelHeight),
                        contentMode: PHImageContentMode.AspectFill,
                        options: new PHImageRequestOptions() { ResizeMode = PHImageRequestOptionsResizeMode.None, DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat, NetworkAccessAllowed = true, Synchronous = true },
                        resultHandler: async (img, info) =>
                        {
                            var res = PHAssetResource.GetAssetResources(phasset);
                            phasset.RequestContentEditingInput(new PHContentEditingInputRequestOptions(), async (s, _) =>
                            {
                                try
                                {
                                    if (img != null)
                                    {
                                        string filePath = string.Empty;

                                        using (img)
                                        {
                                            using (var stream = img.AsJpegStream())
                                            {
                                                var newFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Guid.NewGuid().ToString() + ".JPG");

                                                using (var fileStream = File.Create(newFilePath))
                                                {
                                                    stream.Seek(0, SeekOrigin.Begin);
                                                    await stream.CopyToAsync(fileStream);
                                                }
                                                filePath = newFilePath;
                                            }
                                        }
                                        resultFilePaths.Add(filePath);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    dropViewModel.IosMultipleImageSelectTask.TrySetResult(false);
                                }
                                finally
                                {
                                    tcs.TrySetResult(true);
                                }
                            });
                        });
                    await tcs.Task;
                    dropViewModel.IosFilePathsFromMultiImageSelect = resultFilePaths;

                    if (!dropViewModel.IosMultipleImageSelectTask.Task.IsCompleted)
                        dropViewModel.IosMultipleImageSelectTask.TrySetResult(true); //first file(s) loaded causes UI to continue to next stage
                    else
                        dropViewModel.IosStageFiles(resultFilePaths); //subsequent files get staged in a "lazy" manner
                }
            };

            var vc = Platform.GetCurrentUIViewController();
            vc.PresentModalViewController(picker, true);
        }

        private static async Task<bool> CheckPhotoPermissionAsync()
        {
            var status = ALAssetsLibrary.AuthorizationStatus;

            if (status == ALAuthorizationStatus.Denied)
            {
                var alert = UIAlertController.Create(Strings.NoPhotoAccessTitle, Strings.NoPhotoAccessMessage, UIAlertControllerStyle.Alert);

                alert.AddAction(UIAlertAction.Create(Strings.Cancel, UIAlertActionStyle.Cancel, null));
                alert.AddAction(UIAlertAction.Create(Strings.Settings, UIAlertActionStyle.Default, action => UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(UIApplication.OpenSettingsUrlString))));

                var vc = Platform.GetCurrentUIViewController();
                await vc.PresentViewControllerAsync(alert, true);

                return false;
            }

            return true;
        }
    }
}

