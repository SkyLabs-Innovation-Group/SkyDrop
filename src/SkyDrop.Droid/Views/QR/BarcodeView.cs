
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Styles;

namespace SkyDrop.Droid.Views.Onboarding
{
    [Activity(Label = "BarcodeView")]
    public class BarcodeView : BaseActivity<BarcodeViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.BarcodeView;

        private ImageView barcodeImageView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var editText = FindViewById<EditText>(Resource.Id.BarcodeEditText);
            editText.AfterTextChanged += async (s, e) => await ShowBarcode(editText.Text);

            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImageView);
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode(string url)
        {
            var matrix = ViewModel.GenerateBarcode(url, barcodeImageView.Width, barcodeImageView.Height);
            var bitmap = await AndroidUtil.BitMatrixToBitmap(matrix);
            barcodeImageView.SetImageBitmap(bitmap);
        }
    }
}

