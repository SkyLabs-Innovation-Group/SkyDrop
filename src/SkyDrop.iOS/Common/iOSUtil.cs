using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using MvvmCross;
using UIKit;
using WebKit;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;

namespace SkyDrop.iOS.Common
{
    public static class IOsUtil
    {
        public static Task<UIImage> BitMatrixToImage(BitMatrix bitMatrix)
        {
            var renderer = new BitmapRenderer();
            return Task.Run(() =>
            {
                //computationally heavy but quick
                return renderer.Render(bitMatrix, BarcodeFormat.QR_CODE, "");
            });
        }

        public static void LayoutInsideWithFrame(this UIView parent, UIView child)
        {
            parent.Add(child);
            child.Frame = new CGRect(0, 0, parent.Frame.Width, parent.Frame.Height);
        }

        public static void LoadLocalImagePreview(string filePath, UIImageView target, CancellationToken token)
        {
            var log = Mvx.IoCProvider.Resolve<ILog>();
            Task.Run(async () =>
            {
                try
                {
                    var uiImage = await ImageService.Instance.LoadStream(
                            c => Task.FromResult((Stream)File.OpenRead(filePath)))
                        .DownSampleInDip()
                        .AsUIImageAsync();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        //prevents race conditions
                        if (token.IsCancellationRequested)
                            return;

                        target.Image = uiImage;
                    });
                }
                catch (Exception ex)
                {
                    log.Error("Error setting SkyFile preview", ex);
                }
            });
        }
    }
}