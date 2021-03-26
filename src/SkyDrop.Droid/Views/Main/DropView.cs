using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Google.Android.Material.Card;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Main
{
    /// <summary>
    /// File transfer screen
    /// </summary>
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class DropView : BaseActivity<DropViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.DropView;

        private const int swipeMarginX = 100;
        private bool isPressed;
        private float tapX, barcodeStartX;
        private MaterialCardView sendButton, receiveButton;
        private ConstraintLayout barcodeContainer;
        private LinearLayout barcodeMenu;
        private ImageView barcodeImageView;

        /// <summary>
        /// Initialize view
        /// </summary>
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("DropView OnCreate()");

            ViewModel.SelectFileAsyncFunc = async () => await AndroidUtil.SelectFile(this);
            ViewModel.SelectImageAsyncFunc = async () => await AndroidUtil.SelectImage(this);
            ViewModel.OpenFileCommand = new MvxCommand<SkyFile>(skyFile => AndroidUtil.OpenFileInBrowser(this, skyFile));
            ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;
            ViewModel.HandleUploadErrorCommand = new MvxCommand(() => AnimateSlideBarcodeOut(false));
            ViewModel.ResetBarcodeCommand = new MvxCommand(ResetBarcode);

            sendButton = FindViewById<MaterialCardView>(Resource.Id.SendFileButton);
            receiveButton = FindViewById<MaterialCardView>(Resource.Id.ReceiveFileButton);
            barcodeContainer = FindViewById<ConstraintLayout>(Resource.Id.BarcodeContainer);
            barcodeMenu = FindViewById<LinearLayout>(Resource.Id.BarcodeMenu);
            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
        }

        /// <summary>
        /// Called after user selects file
        /// </summary>
        protected override async void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            try
            {
                if (requestCode == AndroidUtil.PickFileRequestCode)
                {
                    if (data == null)
                    {
                        //user did not select a file, reset UI
                        ViewModel.ResetUI();
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

        /// <summary>
        /// Hide barcode when user presses hardware back button
        /// </summary>
        public override void OnBackPressed()
        {
            if (ViewModel.IsBarcodeVisible)
            {
                AnimateSlideBarcodeOut(false);
                return;
            }

            base.OnBackPressed();
        }

        /// <summary>
        /// Adds settings button to toolbar
        /// </summary>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.DropMenu, menu);
            return true;
        }

        /// <summary>
        /// Navigate to settings when settings button is tapped
        /// </summary>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_drop_settings:
                    ViewModel.NavToSettingsCommand?.Execute();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Generate SkyFile from intent data and stage it for upload
        /// </summary>
        private async Task HandlePickedFile(Intent data)
        {
            AnimateSlideSendButton();
            var stagedFile = await AndroidUtil.HandlePickedFile(this, data);
            await ViewModel.StageFile(stagedFile);
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            ViewModel.IsBarcodeVisible = true;
            AnimateSlideBarcodeIn();
            var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileJson, barcodeImageView.Width, barcodeImageView.Height);
            var bitmap = await AndroidUtil.EncodeBarcode(matrix, barcodeImageView.Width, barcodeImageView.Height);
            barcodeImageView.SetImageBitmap(bitmap);
        }

        /// <summary>
        /// Display grey placeholder QR code
        /// </summary>
        private void ResetBarcode()
        {
            barcodeImageView.SetImageResource(Resource.Drawable.barcode_grey);
        }

        /// <summary>
        /// Slide send button to center
        /// </summary>
        private void AnimateSlideSendButton()
        {
            var screenCenterX = Resources.DisplayMetrics.WidthPixels / 2;
            var sendButtonLocation = new[] { 0, 0 };
            sendButton.GetLocationOnScreen(sendButtonLocation);
            var sendButtonCenterX = sendButtonLocation[0] + sendButton.Width / 2;

            var duration = 1000;
            var translationX = screenCenterX - sendButtonCenterX;
            sendButton.Animate()
                .TranslationX(translationX)
                .SetDuration(duration)
                .Start();
            receiveButton.Animate()
                .Alpha(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Slide in the QR code from the right
        /// </summary>
        private void AnimateSlideBarcodeIn()
        {
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            barcodeContainer.TranslationX = screenWidth;
            barcodeMenu.TranslationX = screenWidth;

            var duration = 666;
            sendButton.Animate()
                .TranslationXBy(-screenWidth)
                .SetDuration(duration)
                .Start();
            barcodeContainer.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
            barcodeMenu.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Slide barcode out to left or right
        /// </summary>
        private void AnimateSlideBarcodeOut(bool toLeft)
        {
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            ViewModel.IsAnimatingBarcodeOut = true;
            ViewModel.IsReceiveButtonGreen = true;
            ViewModel.UploadTimerText = "";
            ViewModel.FileSize = "";

            if (toLeft)
                sendButton.TranslationX = screenWidth;

            var duration = 250;
            sendButton.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .WithEndAction(new Java.Lang.Runnable(() => ViewModel.ResetUI()))
                .Start();
            receiveButton.Animate()
                .Alpha(1)
                .SetDuration(duration)
                .Start();
            barcodeContainer.Animate()
                .TranslationX(toLeft ? -screenWidth : screenWidth)
                .SetDuration(duration)
                .Start();
            barcodeMenu.Animate()
                .TranslationX(toLeft ? -screenWidth : screenWidth)
                .SetDuration(duration)
                .Start();
        }

        private void AnimateSlideBarcodeToCenter()
        {
            ViewModel.IsBarcodeVisible = true;

            var duration = 500;
            barcodeContainer.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
            barcodeMenu.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Intercept touch events for the whole screen to handle swipe gestures
        /// </summary>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    isPressed = true;

                    tapX = e.GetX();

                    barcodeStartX = barcodeContainer.TranslationX;
                    break;

                case MotionEventActions.Up:
                    if (!isPressed)
                        return base.DispatchTouchEvent(e);

                    isPressed = false;

                    if (ViewModel.IsBarcodeVisible && !ViewModel.IsAnimatingBarcodeOut)
                    {
                        if (barcodeContainer.TranslationX >= swipeMarginX)
                            AnimateSlideBarcodeOut(false);
                        else if (barcodeContainer.TranslationX <= -swipeMarginX)
                            AnimateSlideBarcodeOut(true);
                        else
                            AnimateSlideBarcodeToCenter();
                    }
                    break;

                case MotionEventActions.Move:
                    if (!isPressed)
                        return base.DispatchTouchEvent(e);

                    var touchX = e.GetX();
                    var deltaX = touchX - tapX;

                    if (ViewModel.IsBarcodeVisible)
                    {
                        barcodeContainer.TranslationX = barcodeStartX + deltaX;
                        barcodeMenu.TranslationX = barcodeStartX + deltaX;
                    }
                    break;
            }

            return base.DispatchTouchEvent(e);
        }
    }
}
