using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;
using ZXing.QrCode;

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
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();

            if (result == null)
            {
                log.Error("MobileBarcodeScanner result was null");
            }

            // Here you were getting a nullReference exception, which broke your app
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
