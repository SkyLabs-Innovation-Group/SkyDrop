using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using UIKit;
using ZXing.Mobile;

namespace SkyDrop.iOS.Views.Contacts
{
    //[MvxModalPresentation(WrapInNavigationController = true, ModalPresentationStyle = UIModalPresentationStyle.Popover)]
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

            View.BackgroundColor = Colors.DarkGrey.ToNative();
            ShowScanner();
            await ShowBarcode();

            var set = CreateBindingSet();
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();
		}

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                var screenDensity = (int)UIScreen.MainScreen.Scale;
                var matrix = ViewModel.GenerateBarcode((int)BarcodeImage.Frame.Width * screenDensity, (int)BarcodeImage.Frame.Height * screenDensity);
                var image = await iOSUtil.BitMatrixToImage(matrix);
                BarcodeImage.Image = image;
            }
            catch (Exception ex)
            {
                ViewModel.Log.Error("Error in ShowBarcode(): ");
                ViewModel.Log.Exception(ex);
            }
        }

        private void ShowScanner()
        {
            var mobileBarcodeScanner = new MobileBarcodeScanner(this);
            scannerView = new ZXingScannerView(new CGRect(0, 0, ScannerContainer.Frame.Width, ScannerContainer.Frame.Height))
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                UseCustomOverlayView = true
                //UseCustomOverlayView = mobileBarcodeScanner.UseCustomOverlay,
                //CustomOverlayView = mobileBarcodeScanner.CustomOverlay
            };

            ScannerContainer.Add(scannerView);

            scannerView.StartScanning(HandleScanResult);
        }

        private void HandleScanResult(ZXing.Result result)
        {
            if (result == null)
                return;

            var userDialogs = Mvx.IoCProvider.Resolve<IUserDialogs>();
            userDialogs.Toast(result.Text);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ViewModel.Close();
        }
    }
}


