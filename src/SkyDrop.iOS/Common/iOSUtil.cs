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
    }
}
