using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Google.Android.Material.Card;
using Java.Interop;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using static Android.Views.View;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class DropView : BaseActivity<DropViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.DropView;

        private bool isPressed;
        private float tapX, barcodeStartX;
        private MaterialCardView sendButton, receiveButton;
        private ConstraintLayout barcodeContainer;
        private FrameLayout barcodeMenu;
        private const int swipeMarginX = 100;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("DropView OnCreate()");

            ViewModel.SelectFileAsyncFunc = async () => await AndroidUtil.SelectFile(this);
            ViewModel.SelectImageAsyncFunc = async () => await AndroidUtil.SelectImage(this);
            ViewModel.OpenFileCommand = new MvxCommand<SkyFile>(skyFile => AndroidUtil.OpenFileInBrowser(this, skyFile));
            ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;

            sendButton = FindViewById<MaterialCardView>(Resource.Id.SendFileButton);
            receiveButton = FindViewById<MaterialCardView>(Resource.Id.ReceiveFileButton);
            barcodeContainer = FindViewById<ConstraintLayout>(Resource.Id.BarcodeContainer);
            barcodeMenu = FindViewById<FrameLayout>(Resource.Id.BarcodeMenu);

            var rootView = FindViewById<ConstraintLayout>(Resource.Id.Root);
            rootView.Touch += HandleTouchEvents;
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

        public override void OnBackPressed()
        {
            if (!ViewModel.IsBarcodeHidden)
            {
                AnimateSlideBarcodeOut();
                return;
            }

            base.OnBackPressed();
        }

        private async Task HandlePickedFile(Intent data)
        {
            AnimateSlideSendButton();
            var stagedFile = await AndroidUtil.HandlePickedFile(this, data);
            await ViewModel.StageFile(stagedFile);
        }

        public async Task ShowBarcode()
        {
            var barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileJson, barcodeImageView.Width, barcodeImageView.Height);
            var bitmap = await AndroidUtil.EncodeBarcode(matrix, barcodeImageView.Width, barcodeImageView.Height);
            barcodeImageView.SetImageBitmap(bitmap);
            AnimateSlideBarcodeIn();
            ViewModel.IsBarcodeHidden = false;
        }

        private void AnimateSlideSendButton()
        {
            var screenCenterX = Resources.DisplayMetrics.WidthPixels / 2;
            var sendButtonLocation = new[] { 0, 0 };
            sendButton.GetLocationOnScreen(sendButtonLocation);
            var sendButtonCenterX = sendButtonLocation[0] + sendButton.Width / 2;

            var duration = 1000;
            var translationX = screenCenterX - sendButtonCenterX;
            sendButton.Animate().TranslationX(translationX).SetDuration(duration).Start();
            receiveButton.Animate().Alpha(0).SetDuration(duration).Start();
        }

        private void AnimateSlideBarcodeIn()
        {
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            barcodeContainer.TranslationX = screenWidth;
            barcodeMenu.TranslationX = screenWidth;

            var duration = 500;
            sendButton.Animate().TranslationXBy(-screenWidth).SetDuration(duration).Start();
            barcodeContainer.Animate().TranslationX(0).SetDuration(duration).Start();
            barcodeMenu.Animate().TranslationX(0).SetDuration(duration).Start();
        }

        private void AnimateSlideBarcodeOut()
        {
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            ViewModel.ReceiveButtonState = true;

            var duration = 500;
            sendButton.Animate().TranslationX(0).SetDuration(duration).WithEndAction(new Java.Lang.Runnable(() => ViewModel.IsBarcodeHidden = false)).Start();
            receiveButton.Animate().Alpha(1).SetDuration(duration).Start();
            barcodeContainer.Animate().TranslationX(screenWidth).SetDuration(duration).Start();
            barcodeMenu.Animate().TranslationX(screenWidth).SetDuration(duration).Start();
        }

        private void HandleTouchEvents(object sender, TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                e.Handled = true;
                isPressed = true;

                tapX = e.Event.GetX();

                barcodeStartX = barcodeContainer.TranslationX;
            }
            else if (e.Event.Action == MotionEventActions.Up)
            {
                e.Handled = true;
                isPressed = false;
            }
            else if (e.Event.Action == MotionEventActions.Move)
            {
                if (!isPressed)
                {
                    e.Handled = false;
                    return;
                }

                var touchX = e.Event.GetX();
                var deltaX = touchX - tapX;

                if (!ViewModel.IsBarcodeHidden && barcodeContainer.TranslationX < swipeMarginX)
                {
                    barcodeContainer.TranslationX = barcodeStartX + deltaX;
                    barcodeMenu.TranslationX = barcodeStartX + deltaX;

                    if (barcodeContainer.TranslationX >= swipeMarginX)
                        AnimateSlideBarcodeOut();
                }
            }
        }
    }
}
