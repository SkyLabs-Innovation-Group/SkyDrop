using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
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
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class DropView : BaseActivity<DropViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.DropView;

        private const int swipeMarginX = 100;
        private bool isPressed;
        private bool barcodeIsLoaded;
        private float tapStartX, barcodeStartX, sendReceiveButtonsContainerStartX;
        private MaterialCardView sendButton, receiveButton;
        private ConstraintLayout barcodeContainer;
        private LinearLayout barcodeMenu, sendReceiveButtonsContainer;
        private ImageView barcodeImageView;

        /// <summary>
        /// Initialize view
        /// </summary>
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("DropView OnCreate()");

            ViewModel.OpenFileCommand = new MvxCommand<SkyFile>(skyFile => AndroidUtil.OpenFileInBrowser(this, skyFile));
            ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;
            ViewModel.HandleUploadErrorCommand = new MvxCommand(() => AnimateSlideBarcodeOut(false));
            ViewModel.ResetBarcodeCommand = new MvxCommand(ResetBarcode);
            ViewModel.OpenFileInBrowserCommand = new MvxCommand(() => AndroidUtil.OpenFileInBrowser(this, ViewModel.SkyFile));
            ViewModel.SlideSendButtonToCenterCommand = new MvxCommand(AnimateSlideSendButton);

            sendButton = FindViewById<MaterialCardView>(Resource.Id.SendFileButton);
            receiveButton = FindViewById<MaterialCardView>(Resource.Id.ReceiveFileButton);
            barcodeContainer = FindViewById<ConstraintLayout>(Resource.Id.BarcodeContainer);
            barcodeMenu = FindViewById<LinearLayout>(Resource.Id.BarcodeMenu);
            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            sendReceiveButtonsContainer = FindViewById<LinearLayout>(Resource.Id.SendReceiveContainer);
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
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            ViewModel.IsBarcodeVisible = true;

            AnimateSlideBarcodeIn(fromLeft: false);
            
            var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileJson, barcodeImageView.Width, barcodeImageView.Height);
            var bitmap = await AndroidUtil.EncodeBarcode(matrix, barcodeImageView.Width, barcodeImageView.Height);
            barcodeImageView.SetImageBitmap(bitmap);
            barcodeIsLoaded = true;
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
        /// Slide in the QR code from the left or right
        /// </summary>
        private void AnimateSlideBarcodeIn(bool fromLeft)
        {
            ViewModel.IsBarcodeVisible = true;

            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            barcodeContainer.TranslationX = fromLeft ? -screenWidth : screenWidth;
            barcodeMenu.TranslationX = fromLeft ? -screenWidth : screenWidth;

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

            sendReceiveButtonsContainer.TranslationX = 0;
            receiveButton.Alpha = 0;

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

        /// <summary>
        /// Return barcode to center when user cancels a dismiss-slide action
        /// </summary>
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
        /// Slide barcode out to left or right
        /// </summary>
        private void AnimateSlideSendReceiveButtonsOut(bool toLeft)
        {
            if (!barcodeIsLoaded)
            {
                AnimateSlideBarcodeToCenter();
                return;
            }

            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            var duration = 250;
            sendReceiveButtonsContainer.Animate()
                .TranslationX(toLeft ? -screenWidth : screenWidth)
                .SetDuration(duration)
                .Start();

            AnimateSlideBarcodeIn(fromLeft: !toLeft);
        }

        /// <summary>
        /// Slide the send receive button to screen center when user cancels swipe back to barcode action
        /// </summary>
        private void AnimateSlideSendReceiveCenter()
        {
            var duration = 500;
            sendReceiveButtonsContainer.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Intercept touch events for the whole screen to handle swipe gestures
        /// </summary>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            //disable swipe while file is uploading
            if (ViewModel.IsUploading)
                return base.DispatchTouchEvent(e);

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    isPressed = true;

                    tapStartX = e.GetX();

                    barcodeStartX = barcodeContainer.TranslationX;
                    sendReceiveButtonsContainerStartX = sendReceiveButtonsContainer.TranslationX;
                    break;

                case MotionEventActions.Up:
                    if (!isPressed)
                        return base.DispatchTouchEvent(e);

                    isPressed = false;

                    if (!ViewModel.IsBarcodeVisible)
                    {
                        //send & receive buttons are visible

                        if (sendReceiveButtonsContainer.TranslationX >= swipeMarginX)
                            AnimateSlideSendReceiveButtonsOut(toLeft: false);
                        else if (sendReceiveButtonsContainer.TranslationX <= -swipeMarginX)
                            AnimateSlideSendReceiveButtonsOut(toLeft: true);
                        else
                            AnimateSlideSendReceiveCenter();
                    }
                    else if (!ViewModel.IsAnimatingBarcodeOut)
                    {
                        //barcode is visible

                        if (barcodeContainer.TranslationX >= swipeMarginX)
                            AnimateSlideBarcodeOut(toLeft: false);
                        else if (barcodeContainer.TranslationX <= -swipeMarginX)
                            AnimateSlideBarcodeOut(toLeft: true);
                        else
                            AnimateSlideBarcodeToCenter();
                    }

                    break;

                case MotionEventActions.Move:
                    if (!isPressed)
                        return base.DispatchTouchEvent(e);

                    var tapEndX = e.GetX();
                    var deltaX = tapEndX - tapStartX;

                    if (ViewModel.IsBarcodeVisible)
                    {
                        barcodeContainer.TranslationX = barcodeStartX + deltaX;
                        barcodeMenu.TranslationX = barcodeStartX + deltaX;
                    }
                    else
                    {
                        sendReceiveButtonsContainer.TranslationX = sendReceiveButtonsContainerStartX + deltaX;
                    }

                    break;
            }

            return base.DispatchTouchEvent(e);
        }
    }
}
