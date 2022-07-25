using System;
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
using SkyDrop.Core.ViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Views.Files;

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
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
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
    }
}
