using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using MvvmCross;
using UIKit;
using WebKit;
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

        public static void LoadLocalImagePreview(string filePath, UIImageView target)
        {
            var log = Mvx.IoCProvider.Resolve<ILog>();
            Task.Run(async () =>
            {
                try
                {
                    var task = ImageService.Instance.LoadStream(
                            c => Task.FromResult((Stream)File.OpenRead(filePath)))
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

        public static async Task ExecuteJs(this WKWebView webView, string js)
        {
            //need to handle the newline chars differently on iOS
            //js = js.Replace(System.Environment.NewLine, @"\n");
            await webView.EvaluateJavaScriptAsync(js);
        }
    }
}