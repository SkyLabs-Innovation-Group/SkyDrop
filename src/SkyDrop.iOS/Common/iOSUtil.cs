using System.Threading.Tasks;
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
    }
}
