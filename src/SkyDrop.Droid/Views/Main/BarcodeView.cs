using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class BarcodeView : BaseActivity<BarcodeViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.BarcodeView;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("BarcodeView OnCreate()");

            ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;
        }

        public async Task ShowBarcode()
        {
            var imageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            var matrix = ViewModel.GenerateBarcode("panchos", imageView.Width, imageView.Height);
            var bitmap = await AndroidUtil.EncodeBarcode(matrix, imageView.Width, imageView.Height);
            imageView.SetImageBitmap(bitmap);
        }
    }
}
