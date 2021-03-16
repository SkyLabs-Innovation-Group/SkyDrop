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
    public class DropView : BaseActivity<DropViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.DropView;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("DropView OnCreate()");

            ViewModel.SelectFileAsyncFunc = async () => await AndroidUtil.SelectFile(this);
            ViewModel.SelectImageAsyncFunc = async () => await AndroidUtil.SelectImage(this);
            ViewModel.OpenFileCommand = new MvxCommand<SkyFile>(skyFile => AndroidUtil.OpenFileInBrowser(this, skyFile));
            ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;
        }

        protected override async void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            try
            {
                if (requestCode == AndroidUtil.PickFileRequestCode)
                {
                    if (data == null)
                    {
                        //user did not select a file, reset UI
                        ViewModel.SendButtonState = true;
                        ViewModel.ReceiveButtonState = true;
                        return;
                    }

                    await HandlePickedFile(data);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async Task HandlePickedFile(Intent data)
        {
            var stagedFile = await AndroidUtil.HandlePickedFile(this, data);
            await ViewModel.StageFile(stagedFile);
        }

        public async Task ShowBarcode()
        {
            var imageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileJson, imageView.Width, imageView.Height);
            var bitmap = await AndroidUtil.EncodeBarcode(matrix, imageView.Width, imageView.Height);
            imageView.SetImageBitmap(bitmap);
            ViewModel.IsBarcodeHidden = false;
        }
    }
}
