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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);

            ShowBarcode();
            ShowScanner();
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                //why delay?
                await Task.Delay(500);

                var matrix = ViewModel.GenerateBarcode(barcodeImageView.Width, barcodeImageView.Height);
                var bitmap = await AndroidUtil.BitMatrixToBitmap(matrix);
                barcodeImageView.SetImageBitmap(bitmap);
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
            var scannerView = new ZXingSurfaceView(this, scanningOptions);
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
