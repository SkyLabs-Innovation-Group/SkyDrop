using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using UIKit;
using Xamarin.Essentials;
using ZXing.Mobile;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.iOS.Views.Contacts
{
    [MvxChildPresentation]
    public partial class SharePublicKeyView : BaseViewController<SharePublicKeyViewModel>
    {
        private ZXingScannerView scannerView;

        public SharePublicKeyView() : base("SharePublicKeyView", null)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.ScanAgainCommand = new MvxCommand(ScanAgain);
            ViewModel.RefreshBarcodeCommand = new MvxAsyncCommand(ShowBarcode);

            View.BackgroundColor = Colors.DarkGrey.ToNative();
            ScannerOverlay.BackgroundColor = Colors.Primary.ToNative();
            ScanAgainButton.BackgroundColor = Colors.GradientGreen.ToNative();
            ShowScanner();
            await ShowBarcode();

            var set = CreateBindingSet();
            set.Bind(ScanAgainButton).For("Tap").To(vm => vm.ScanAgainCommand);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Bind(this).For(t => t.AddContactResult).To(vm => vm.AddContactResult);
            set.Apply();
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var screenDensity = (int)UIScreen.MainScreen.Scale;
                    var matrix = ViewModel.GenerateBarcode((int)BarcodeImage.Frame.Width * screenDensity, (int)BarcodeImage.Frame.Height * screenDensity);
                    var image = await iOSUtil.BitMatrixToImage(matrix);
                    BarcodeImage.Image = image;
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
            scannerView = new ZXingScannerView(new CGRect(0, 0, ScannerContainer.Frame.Width, ScannerContainer.Frame.Height))
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                UseCustomOverlayView = true //hides red line overlay
            };

            ScannerContainer.Add(scannerView);

            scannerView.StartScanning(HandleScanResult, new MobileBarcodeScanningOptions { });
        }

        private void HandleScanResult(ZXing.Result result)
        {
            if (result == null)
                return;

            ViewModel.AddContact(result.Text);
        }

        public AddContactResult AddContactResult
        {
            get => AddContactResult.Default;
            set
            {
                var overlayVisible = value == AddContactResult.ContactAdded || value == AddContactResult.AlreadyExists;
                ScannerOverlay.Hidden = !overlayVisible;
                ScannerContainer.BringSubviewToFront(ScannerOverlay);
            }
        }

        private void ScanAgain()
        {
            ViewModel.AddContactResult = AddContactResult.Default; //reset ui
        }
    }
}


