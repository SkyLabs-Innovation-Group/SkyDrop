using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using UIKit;

namespace SkyDrop.iOS.Views.Contacts
{
	[MvxModalPresentation(WrapInNavigationController = true, ModalPresentationStyle = UIModalPresentationStyle.Popover)]
	public partial class SharePublicKeyView : MvxViewController<SharePublicKeyViewModel>
	{
		public SharePublicKeyView() : base("SharePublicKeyView", null)
		{
		}

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();

            await ShowBarcode();

            View.BackgroundColor = Colors.DarkGrey.ToNative();
		}

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                var matrix = ViewModel.GenerateBarcode((int)BarcodeImage.Frame.Width, (int)BarcodeImage.Frame.Height);
                var image = await iOSUtil.BitMatrixToImage(matrix);
                BarcodeImage.Image = image;
            }
            catch (Exception ex)
            {
                ViewModel.Log.Error("Error in ShowBarcode(): ");
                ViewModel.Log.Exception(ex);
            }
        }
    }
}


