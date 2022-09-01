using System;
using System.IO;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Cross;
using MvvmCross;
using MvvmCross.Platforms.Ios.Views;
using UIKit;
using ZXing.Common;
using ZXing.Mobile;

namespace SkyDrop.iOS.Common
{
    public static class iOSUtil
    {
        public static Task<UIImage> BitMatrixToImage(BitMatrix bitMatrix)
        {
            var renderer = new BitmapRenderer();
            return Task.Run(() =>
            {
                //computationally heavy but quick
                return renderer.Render(bitMatrix, ZXing.BarcodeFormat.QR_CODE, "");
            });
        }

        public static void LayoutInsideWithFrame(this UIView parent, UIView child)
        {
            parent.Add(child);
            child.Frame = new CoreGraphics.CGRect(0, 0, parent.Frame.Width, parent.Frame.Height);
        }

        public static void LoadLocalImagePreview(string filePath, UIImageView target)
        {
            var log = Mvx.IoCProvider.Resolve<ILog>();
            Task.Run(async () =>
            {
                try
                {
                    var task = ImageService.Instance.LoadStream(
                        c => Task.FromResult((Stream)System.IO.File.OpenRead(filePath)))
                        .DownSampleInDip()
                        .IntoAsync(target);

                    await task;
                }
                catch (Exception ex)
                {
                    log.Error("Error setting SkyFile preview", ex);
                }
            });
        }
    }
}
