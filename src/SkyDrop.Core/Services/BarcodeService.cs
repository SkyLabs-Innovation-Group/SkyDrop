using System.Threading.Tasks;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;
using PermissionStatus = Xamarin.Essentials.PermissionStatus;

namespace SkyDrop.Core.Services
{
    public class BarcodeService : IBarcodeService
    {
        private readonly ILog log;

        public BarcodeService(ILog log)
        {
            this.log = log;
        }

        public async Task<string> ScanBarcode()
        {
            var permissionResult = await Permissions.RequestAsync<Permissions.Camera>();

            if (permissionResult != PermissionStatus.Granted)
            {
                log.Error("Camera permission not granted.");
                return null;
            }
            else
            {
                log.Trace("ScanBarcode() was called, with camera permission granted");
            }
                
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();

            if (result == null)
            {
                log.Error("MobileBarcodeScanner result was null");
            }

            return result?.Text;
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            var writer = new MultiFormatWriter();
            var matrix = writer.encode(text, BarcodeFormat.QR_CODE, width, height);
            return matrix;
        }
    }

    public interface IBarcodeService
    {
        Task<string> ScanBarcode();

        BitMatrix GenerateBarcode(string text, int width, int height);
    }
}
