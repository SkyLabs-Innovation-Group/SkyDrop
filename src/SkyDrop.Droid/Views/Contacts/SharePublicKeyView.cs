using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Commands;
using SkyDrop.Core.ViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Views.Files;
using Xamarin.Essentials;
using ZXing.Mobile;
using static ZXing.Mobile.MobileBarcodeScanningOptions;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class SharePublicKeyView : BaseActivity<SharePublicKeyViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.SharePublicKeyView;

        private ImageView barcodeImageView;
        private ZXingSurfaceView scannerView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ViewModel.RefreshBarcodeCommand = new MvxAsyncCommand(ShowBarcode);
            ViewModel.HideKeyboardCommand = new MvxCommand(this.HideKeyboard);

            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);

            ShowScanner();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            scannerView.StopScanning();
            scannerView.Dispose();
        }

        /// <summary>
        /// Generate and display QR code
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
            var scanningOptions = new MobileBarcodeScanningOptions { CameraResolutionSelector = new CameraResolutionSelectorDelegate(QRScannerHelper.GetSquareScannerResolution) };
            scannerView = new ZXingSurfaceView(this, scanningOptions);
            scannerView.LayoutParameters = new MaterialCardView.LayoutParams(MaterialCardView.LayoutParams.MatchParent, MaterialCardView.LayoutParams.MatchParent);

            var scannerContainer = FindViewById<MaterialCardView>(Resource.Id.ScannerContainer);
            scannerContainer.AddView(scannerView);

            scannerView.StartScanning(HandleScanResult, scanningOptions);
        }

        private void HandleScanResult(ZXing.Result result)
        {
            if (result == null)
                return;

            ViewModel.AddContact(result.Text);
        }
    }
}
