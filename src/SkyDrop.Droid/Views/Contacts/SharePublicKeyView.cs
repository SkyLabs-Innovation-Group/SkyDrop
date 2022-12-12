using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Commands;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Helper;
using Xamarin.Essentials;
using ZXing.Mobile;
using Result = ZXing.Result;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class SharePublicKeyView : BaseActivity<SharePublicKeyViewModel>
    {
        private ImageView barcodeImageView;
        private ZXingSurfaceView scannerView;
        protected override int ActivityLayoutId => Resource.Layout.SharePublicKeyView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ViewModel.RefreshBarcodeCommand = new MvxAsyncCommand(ShowBarcode);
            ViewModel.StopScanningCommand = new MvxCommand(StopScanning);

            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);

            ShowScanner();
            ShowBarcode();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            StopScanning();
        }

        /// <summary>
        ///     Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                //delay so we can get the correct barcodeImageView size
                await Task.Delay(200);

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var matrix = ViewModel.GenerateBarcode(barcodeImageView.Width, barcodeImageView.Height);
                    var bitmap = await AndroidUtil.BitMatrixToBitmap(matrix);
                    barcodeImageView.SetImageBitmap(bitmap);
                });
            }
            catch (Exception ex)
            {
                ViewModel.Log.Error("Error in ShowBarcode(): ");
                ViewModel.Log.Exception(ex);
            }
        }

        private void ShowScanner()
        {
            var scanningOptions = new MobileBarcodeScanningOptions
                { CameraResolutionSelector = QrScannerHelper.GetSquareScannerResolution };
            scannerView = new ZXingSurfaceView(this, scanningOptions);
            scannerView.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);

            var scannerContainer = FindViewById<MaterialCardView>(Resource.Id.ScannerContainer);
            scannerContainer.AddView(scannerView);

            scannerView.StartScanning(HandleScanResult, scanningOptions);
        }

        private void StopScanning()
        {
            scannerView.StopScanning();
            scannerView.Dispose();
        }

        private void HandleScanResult(Result result)
        {
            if (result == null)
                return;

            ViewModel.AddContact(result.Text);
        }
    }
}