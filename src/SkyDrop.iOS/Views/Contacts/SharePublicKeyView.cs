using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using UIKit;
using Xamarin.Essentials;
using ZXing;
using ZXing.Mobile;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.iOS.Views.Contacts
{
    [MvxChildPresentation]
    public partial class SharePublicKeyView : BaseViewController<SharePublicKeyViewModel>
    {
        private UIBarButtonItem nextButton;
        private ZXingScannerView scannerView;

        public SharePublicKeyView() : base("SharePublicKeyView", null)
        {
        }

        public AddContactResult AddContactResult
        {
            get => AddContactResult.Default;
            set
            {
                var isSuccess = value == AddContactResult.ContactAdded || value == AddContactResult.AlreadyExists ||
                                value == AddContactResult.DevicesPaired;
                var isError = value == AddContactResult.InvalidKey || value == AddContactResult.WrongDevice;
                var overlayVisible = isSuccess || isError;
                var overlayColor = isError ? Colors.Red : Colors.Primary;
                ScannerOverlay.Hidden = !overlayVisible;
                ScannerOverlay.BackgroundColor = overlayColor.ToNative();
                ScannerContainer.BringSubviewToFront(ScannerOverlay);
                StatusIcon.Image = isError ? UIImage.FromBundle("ic_error") : UIImage.FromBundle("ic_tick");
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddBackButton(() => ViewModel.Close());

            ViewModel.RefreshBarcodeCommand = new MvxAsyncCommand(ShowBarcode);
            ViewModel.StopScanningCommand = new MvxCommand(StopScanning);

            View.BackgroundColor = Colors.DarkGrey.ToNative();
            ScannerOverlay.BackgroundColor = Colors.Primary.ToNative();
            ShowScanner();
            ShowBarcode();

            var set = CreateBindingSet();
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Bind(this).For(t => t.AddContactResult).To(vm => vm.AddContactResult);
            set.Bind(HintLabel).To(vm => vm.HintText);
            set.Apply();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            StopScanning();
        }

        /// <summary>
        ///     Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var screenDensity = (int)UIScreen.MainScreen.Scale;
                    var matrix = ViewModel.GenerateBarcode((int)BarcodeImage.Frame.Width * screenDensity,
                        (int)BarcodeImage.Frame.Height * screenDensity);
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
            scannerView =
                new ZXingScannerView(new CGRect(0, 0, ScannerContainer.Frame.Width, ScannerContainer.Frame.Height))
                {
                    AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                    UseCustomOverlayView = true //hides red line overlay
                };

            ScannerContainer.Add(scannerView);

            scannerView.StartScanning(HandleScanResult, new MobileBarcodeScanningOptions());
        }

        private void HandleScanResult(Result result)
        {
            if (result == null)
                return;

            ViewModel.AddContact(result.Text);
        }

        private void StopScanning()
        {
            scannerView.StopScanning();
            scannerView.Dispose();
        }
    }
}