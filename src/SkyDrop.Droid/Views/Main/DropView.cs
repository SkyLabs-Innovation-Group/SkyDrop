using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Google.Android.Material.Card;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using ZXing.Mobile;
using static SkyDrop.Core.ViewModels.Main.DropViewModel;
using static SkyDrop.Core.Utility.Util;
using MvvmCross;
using SkyDrop.Core.Services;
using System.IO;
using FFImageLoading.Cross;
using Android;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using Java.IO;
using Plugin.CurrentActivity;

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
        private float tapStartX, barcodeStartX, sendReceiveButtonsContainerStartX;
        private MaterialCardView sendButton, receiveButton;
        private ConstraintLayout barcodeContainer, sendReceiveButtonsContainer;
        private LinearLayout barcodeMenu, homeMenu;
        private ImageView barcodeImageView;
        private View leftDot, rightDot;

        /// <summary>
        /// Initialize view
        /// </summary>
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            CrossCurrentActivity.Current.Init(this, bundle);

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            await ViewModel.InitializeTask.Task;

            Log.Trace("DropView OnCreate()");

            ViewModel.GenerateBarcodeAsyncFunc = t => ShowBarcode(t);
            ViewModel.ResetUIStateCommand = new MvxCommand(() => SetSendReceiveButtonUiState());
            ViewModel.SlideSendButtonToCenterCommand = new MvxCommand(AnimateSlideSendButton);
            ViewModel.SlideReceiveButtonToCenterCommand = new MvxCommand(AnimateSlideReceiveButton);
            ViewModel.CheckUserIsSwipingCommand = new MvxCommand(CheckUserIsSwiping);
            ViewModel.UpdateNavDotsCommand = new MvxCommand(() => UpdateNavDots());
            ViewModel.UploadStartedNotificationCommand = new MvxCommand(() => AndroidUtil.ShowUploadStartedNotification(this, $"{ViewModel.FileToUpload.Filename} {ViewModel.FileSize}"));
            ViewModel.UploadFinishedNotificationCommand = new MvxCommand<FileUploadResult>(result => AndroidUtil.ShowUploadFinishedNotification(this, result));
            ViewModel.UpdateNotificationProgressCommand = new MvxCommand<double>(progress => AndroidUtil.UpdateNotificationProgress(this, progress));

            var fileSystemService = Mvx.IoCProvider.Resolve<IFileSystemService>();
            fileSystemService.DownloadsFolderPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            fileSystemService.CacheFolderPath = System.IO.Path.GetTempPath();

            sendButton = FindViewById<MaterialCardView>(Resource.Id.SendFileButton);
            receiveButton = FindViewById<MaterialCardView>(Resource.Id.ReceiveFileButton);
            barcodeContainer = FindViewById<ConstraintLayout>(Resource.Id.BarcodeContainer);
            barcodeMenu = FindViewById<LinearLayout>(Resource.Id.BarcodeMenu);
            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            sendReceiveButtonsContainer = FindViewById<ConstraintLayout>(Resource.Id.SendReceiveContainer);
            homeMenu = FindViewById<LinearLayout>(Resource.Id.HomeMenu);

            var stagedFilesRecycler = FindViewById<RecyclerView>(Resource.Id.StagedFilesRecycler);
            stagedFilesRecycler.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));

            CreateNavDots();
        }

        /// <summary>
        /// Hide barcode when user presses hardware back button
        /// </summary>
        public override void OnBackPressed()
        {
            if (ViewModel.IsBarcodeVisible)
            {
                SetSendReceiveButtonUiState();
                return;
            }

            base.OnBackPressed();
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode(string url)
        {
            SetBarcodeCodeUiState(isSlow: true);

            var matrix = ViewModel.GenerateBarcode(url, barcodeImageView.Width, barcodeImageView.Height);
            var bitmap = await AndroidUtil.BitMatrixToBitmap(matrix);
            barcodeImageView.SetImageBitmap(bitmap);
            ViewModel.SwipeNavigationEnabled = true;
        }

        /// <summary>
        /// Return to the initial UI state
        /// </summary>
        private void SetSendReceiveButtonUiState()
        {
            //DropViewUIState gets changed at the end of the animation 
            //that is to fix an issue with CheckUserIsSwiping() on barcode menu buttons
            AnimateSlideBarcodeOut();

            ViewModel.Title = "SkyDrop";
        }

        /// <summary>
        /// Show the QR code UI state
        /// </summary>
        private void SetBarcodeCodeUiState(bool isSlow = false)
        {
            ViewModel.DropViewUIState = DropViewState.QRCodeState;
            ViewModel.Title = ViewModel.FocusedFile?.Filename;
            ViewModel.IsBarcodeVisible = true;

            AnimateSlideBarcodeIn(fromLeft: false, isSlow);
            AnimateSlideSendReceiveButtonsOut(toLeft: true);
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
            homeMenu.Animate()
                .Alpha(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Slide receive button to center
        /// </summary>
        private void AnimateSlideReceiveButton()
        {
            var screenCenterX = Resources.DisplayMetrics.WidthPixels / 2;
            var receiveButtonLocation = new[] { 0, 0 };
            receiveButton.GetLocationOnScreen(receiveButtonLocation);
            var receiveButtonCenterX = receiveButtonLocation[0] + receiveButton.Width / 2;

            var duration = 1000;
            var translationX = screenCenterX - receiveButtonCenterX;
            receiveButton.Animate()
                .TranslationX(translationX)
                .SetDuration(duration)
                .Start();
            sendButton.Animate()
                .Alpha(0)
                .SetDuration(duration)
                .Start();
            homeMenu.Animate()
                .Alpha(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Slide in the QR code from the left or right
        /// </summary>
        private void AnimateSlideBarcodeIn(bool fromLeft, bool isSlow)
        {
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            barcodeContainer.TranslationX = fromLeft ? -screenWidth : screenWidth;
            barcodeMenu.TranslationX = fromLeft ? -screenWidth : screenWidth;

            var duration = isSlow ? 666 : 250;
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
        /// Slide barcode out, slide send receive buttons in
        /// </summary>
        private void AnimateSlideBarcodeOut()
        {
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            ViewModel.IsAnimatingBarcodeOut = true;
            ViewModel.IsReceiveButtonGreen = true;
            ViewModel.UploadTimerText = "";
            ViewModel.FileSize = "";

            sendReceiveButtonsContainer.TranslationX = -screenWidth;
            sendButton.Alpha = 1;
            sendButton.TranslationX = 0;
            receiveButton.Alpha = 1;
            receiveButton.TranslationX = 0;

            var duration = 250;
            sendReceiveButtonsContainer.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .WithEndAction(new Java.Lang.Runnable(() => { ViewModel.DropViewUIState = DropViewState.SendReceiveButtonState; ViewModel.ResetUI(); }))
                .Start();
            barcodeContainer.Animate()
                .TranslationX(screenWidth)
                .SetDuration(duration)
                .Start();
            barcodeMenu.Animate()
                .TranslationX(screenWidth)
                .SetDuration(duration)
                .Start();
            homeMenu.Animate()
                .TranslationX(0)
                .Alpha(1)
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
        /// Slide send receive buttons out to left or right
        /// </summary>
        private void AnimateSlideSendReceiveButtonsOut(bool toLeft)
        {
            if (!ViewModel.SwipeNavigationEnabled)
                AnimateSlideBarcodeToCenter();

            var screenWidth = Resources.DisplayMetrics.WidthPixels;
            var duration = 250;
            var translationX = toLeft ? -screenWidth : screenWidth;
            sendReceiveButtonsContainer.Animate()
                .TranslationX(translationX)
                .SetDuration(duration)
                .Start();
            homeMenu.Animate()
                .TranslationX(translationX)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Slide the send receive buttons to screen center when user cancels swipe back to barcode action
        /// </summary>
        private void AnimateSlideSendReceiveCenter()
        {
            var duration = 500;
            sendReceiveButtonsContainer.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
            homeMenu.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
        }

        /// <summary>
        /// Intercept touch events for the whole screen to handle swipe gestures
        /// </summary>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (!ViewModel.SwipeNavigationEnabled || //don't allow swipe before first file is uploaded
                ViewModel.IsUploading || //don't allow swipe while file is uploading
                ViewModel.DropViewUIState == DropViewState.ConfirmFilesState) //don't allow swipe on confirm file UI state
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

                        if (sendReceiveButtonsContainer.TranslationX <= -swipeMarginX)
                            SetBarcodeCodeUiState();
                        else
                            AnimateSlideSendReceiveCenter();
                    }
                    else if (!ViewModel.IsAnimatingBarcodeOut)
                    {
                        //barcode is visible

                        if (barcodeContainer.TranslationX >= swipeMarginX)
                            SetSendReceiveButtonUiState();
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
                        var translationX = sendReceiveButtonsContainerStartX + deltaX;
                        sendReceiveButtonsContainer.TranslationX = translationX;
                        homeMenu.TranslationX = translationX;
                    }

                    break;
            }

            return base.DispatchTouchEvent(e);
        }

        /// <summary>
        /// Checks whether the user is doing a swipe and returns the result to the VM
        /// </summary>
        private void CheckUserIsSwiping()
        {
            var thresholdOffset = AndroidUtil.DpToPx(16);
            var isSendReceiveButtonsCentered = sendReceiveButtonsContainer.TranslationX >= -thresholdOffset && sendReceiveButtonsContainer.TranslationX <= thresholdOffset;
            var isBarcodeCentered = barcodeContainer.TranslationX >= -thresholdOffset && barcodeContainer.TranslationX <= thresholdOffset;
            var interfaceIsCentered = ViewModel.DropViewUIState == DropViewState.SendReceiveButtonState ? isSendReceiveButtonsCentered : isBarcodeCentered;
            var userIsSwipingResult = !interfaceIsCentered;
            ViewModel.Log.Trace($"UserIsSwipingResult: {userIsSwipingResult}");
            ViewModel.UserIsSwipingResult = userIsSwipingResult;
        }

        /// <summary>
        /// Create navigation dots display
        /// </summary>
        private void CreateNavDots()
        {
            var dotSize = 40;

            leftDot = new View(this) { Alpha = NavDotsMaxAlpha, LayoutParameters = new LinearLayout.LayoutParams(dotSize, dotSize) { RightMargin = dotSize } };
            rightDot = new View(this) { Alpha = NavDotsMinAlpha, LayoutParameters = new LinearLayout.LayoutParams(dotSize, dotSize) };
            leftDot.Background = GetDrawable(Resource.Drawable.ic_circle);
            rightDot.Background = GetDrawable(Resource.Drawable.ic_circle);

            var dotsContainer = FindViewById<LinearLayout>(Resource.Id.NavDotsLayout);
            dotsContainer.AddView(leftDot);
            dotsContainer.AddView(rightDot);
        }

        /// <summary>
        /// Change alpha of navigation dots display to reflect new UI state
        /// </summary>
        private void UpdateNavDots()
        {
            var duration = 250; //ms

            switch (ViewModel.DropViewUIState)
            {
                case DropViewState.SendReceiveButtonState:
                case DropViewState.ConfirmFilesState:
                    leftDot.Animate().Alpha(NavDotsMaxAlpha).SetDuration(duration).Start();
                    rightDot.Animate().Alpha(NavDotsMinAlpha).SetDuration(duration).Start();
                    break;
                case DropViewState.QRCodeState:
                    leftDot.Animate().Alpha(NavDotsMinAlpha).SetDuration(duration).Start();
                    rightDot.Animate().Alpha(NavDotsMaxAlpha).SetDuration(duration).Start();
                    break;
            }
        }
    }
}
