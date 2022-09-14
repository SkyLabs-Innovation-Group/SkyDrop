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
        private UIBarButtonItem nextButton;

        public SharePublicKeyView() : base("SharePublicKeyView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddBackButton(() => ViewModel.Close());

            ViewModel.RefreshBarcodeCommand = new MvxAsyncCommand(ShowBarcode);
            ViewModel.HideKeyboardCommand = new MvxCommand(() => View.EndEditing(true));
            ViewModel.StopScanningCommand = new MvxCommand(StopScanning);

            View.BackgroundColor = Colors.DarkGrey.ToNative();
            ScannerOverlay.BackgroundColor = Colors.Primary.ToNative();
            ShowScanner();

            NameInput.Placeholder = "Contact name";
            nextButton = new UIBarButtonItem("Next", UIBarButtonItemStyle.Plain, (s, e) =>
            {
                ViewModel.ConfirmContactNameCommand.Execute();
            });

            var set = CreateBindingSet();
            set.Bind(ScannerContainer).For(a => a.Hidden).To(vm => vm.IsNameInputVisible);
            set.Bind(BarcodeImage).For(a => a.Hidden).To(vm => vm.IsNameInputVisible);
            set.Bind(NameInput).For("Visible").To(vm => vm.IsNameInputVisible);
            set.Bind(NameInput).For(c => c.Text).To(vm => vm.ContactName);

            set.Bind(this).For(t => t.NextButtonVisibility).To(vm => vm.IsNextButtonVisible);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Bind(this).For(t => t.AddContactResult).To(vm => vm.AddContactResult);
            set.Apply();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            StopScanning();
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
                var overlayVisible = value == AddContactResult.ContactAdded || value == AddContactResult.AlreadyExists || value == AddContactResult.DevicesPaired;
                ScannerOverlay.Hidden = !overlayVisible;
                ScannerContainer.BringSubviewToFront(ScannerOverlay);
            }
        }

        public bool NextButtonVisibility
        {
            get => false;
            set
            {
                NavigationItem.RightBarButtonItem = value ? nextButton : null;
            }
        }

        private void StopScanning()
        {
            scannerView.StopScanning();
            scannerView.Dispose();
        }
    }
}


