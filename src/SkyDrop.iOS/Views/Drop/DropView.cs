using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using Engage.iOS.Bindings;
using Foundation;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.iOS.Common;
using UIKit;
using ZXing.Mobile;
using ZXing.Rendering;
using static SkyDrop.Core.ViewModels.Main.DropViewModel;

namespace SkyDrop.iOS.Views.Drop
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class DropView : MvxViewController<DropViewModel>
    {
        private const int swipeMarginX = 20;
        private bool isPressed;
        private nfloat tapStartX, barcodeStartX, sendReceiveButtonsContainerStartX;

        public DropView() : base("DropView", null)
        {
        }

        //what is this?
        partial void DropViewClickAction(NSObject sender)
        {
            try
            {
                ViewModel.NavToSettingsCommand.Execute();
            }
            catch (Exception ex)
            {
                ViewModel.Log.Exception(ex);
            }
        }

        public override void ViewDidLoad()
        {
            try
            {
                base.ViewDidLoad();

                ViewModel.SlideSendButtonToCenterCommand = new MvxCommand(AnimateSlideSendButton);
                ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;

                SetupGestureListener();

                //setup nav bar
                NavigationController.NavigationBar.BarTintColor = Colors.GradientDark.ToNative();
                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes()
                {
                    ForegroundColor = Colors.LightGrey.ToNative()
                };

                View.BackgroundColor = Colors.DarkGrey.ToNative();

                SendButton.BackgroundColor = Colors.GradientGreen.ToNative();
                ReceiveButton.BackgroundColor = Colors.GradientOcean.ToNative();
                SendButton.Layer.CornerRadius = 8;
                ReceiveButton.Layer.CornerRadius = 8;

                CopyLinkButton.BackgroundColor = Colors.Primary.ToNative();
                OpenButton.BackgroundColor = Colors.GradientGreen.ToNative();
                ShareButton.BackgroundColor = Colors.GradientOcean.ToNative();
                CopyLinkButton.Layer.CornerRadius = 8;
                OpenButton.Layer.CornerRadius = 8;
                ShareButton.Layer.CornerRadius = 8;

                ProgressFillArea.BackgroundColor = Colors.GradientTurqouise.ToNative();
                ProgressFillArea.Layer.CornerRadius = 8;

                BarcodeContainer.Layer.CornerRadius = 8;
                BarcodeContainer.ClipsToBounds = true;

                var set = CreateBindingSet();

                //setup file preview collection view
                var filePreviewSource = new MvxCollectionViewSource(FilePreviewCollectionView, FilePreviewCollectionViewCell.Key);
                FilePreviewCollectionView.DataSource = filePreviewSource;
                FilePreviewCollectionView.RegisterClassForCell(typeof(FilePreviewCollectionViewCell), FilePreviewCollectionViewCell.Key);
                set.Bind(filePreviewSource).For(s => s.ItemsSource).To(vm => vm.StagedFiles);
                set.Bind(FilePreviewCollectionView).For("Visible").To(vm => vm.IsStagedFilesVisible);

                set.Bind(SendButton).For("Tap").To(vm => vm.SendCommand);
                set.Bind(ReceiveButton).For("Tap").To(vm => vm.ReceiveCommand);

                set.Bind(CopyLinkButton).For("Tap").To(vm => vm.CopyLinkCommand);
                set.Bind(OpenButton).For("Tap").To(vm => vm.OpenFileInBrowserCommand);
                set.Bind(ShareButton).For("Tap").To(vm => vm.ShareCommand);

                set.Bind(this).For(th => th.Title).To(vm => vm.Title);

                set.Bind(BarcodeMenu).For("Visible").To(vm => vm.IsBarcodeVisible);
                set.Bind(BarcodeContainer).For("Visible").To(vm => vm.IsBarcodeVisible);

                set.Bind(ActivityIndicator).For("Visible").To(vm => vm.IsUploading);
                set.Bind(SendIcon).For(v => v.Hidden).To(vm => vm.IsUploading);

                set.Bind(SendLabel).To(vm => vm.SendButtonLabel);

                set.Bind(FileSizeLabel).To(vm => vm.FileSize);

                set.Bind(ProgressFillArea).For(ProgressFillHeightBinding.Name).To(vm => vm.UploadProgress);

                set.Apply();
            }
            catch(Exception e)
            {
                ViewModel.Log.Exception(e);
            }
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            SetBarcodeCodeUiState();

            var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileJson, (int)BarcodeImage.Frame.Width, (int)BarcodeImage.Frame.Height);
            var renderer = new BitmapRenderer();
            var image = await Task.Run(() =>
            {
                //computationally heavy but quick
                return renderer.Render(matrix, ZXing.BarcodeFormat.QR_CODE, ViewModel.SkyFileJson);
            });

            BarcodeImage.Image = image;
        }

        /// <summary>
        /// Return to the initial UI state
        /// </summary>
        private void SetSendReceiveButtonUiState()
        {
            //DropViewUIState gets changed at the end of the animation 
            //that is to fix an issue with CheckUserIsSwiping() on barcode menu buttons
            AnimateSlideBarcodeOut(toLeft: false);
        }

        /// <summary>
        /// Show the QR code UI state
        /// </summary>
        private void SetBarcodeCodeUiState()
        {
            ViewModel.DropViewUIState = DropViewState.QRCodeState;

            ViewModel.IsBarcodeVisible = true;

            AnimateSlideBarcodeIn(fromLeft: false);
            AnimateSlideSendReceiveButtonsOut(toLeft: true);
        }
        
        /// <summary>
        /// Slide send button to center
        /// </summary>
        private void AnimateSlideSendButton()
        {
            var screenCenterX = UIScreen.MainScreen.Bounds.Width / 2;
            var sendButtonLocation = new[] { 0, 0 };

            var sendButtonCenterX = SendButton.ConvertPointToView(new CGPoint(SendButton.Bounds.Width * 0.5, SendButton.Bounds.Height), null).X;
            var translationX = screenCenterX - sendButtonCenterX;

            var sendFrame = SendButton.Frame;
            UIView.Animate(1, () =>
            {
                SendButton.Frame = new CGRect(sendFrame.X + translationX, sendFrame.Y, sendFrame.Width, sendFrame.Height);
                ReceiveButton.Alpha = 0;
            });
        }

        /// <summary>
        /// Slide in the QR code from the left or right
        /// </summary>
        private void AnimateSlideBarcodeIn(bool fromLeft)
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Width;
            var screenCenterX = screenWidth * 0.5;

            var barcodeTranslationX = fromLeft ? -screenWidth : screenWidth;

            var barcodeMenuFrame = BarcodeMenu.Frame;
            var barcodeContainerFrame = BarcodeContainer.Frame;
            BarcodeMenu.Frame = new CGRect(screenCenterX - barcodeMenuFrame.Width * 0.5 + screenWidth, barcodeMenuFrame.Y, barcodeMenuFrame.Width, barcodeMenuFrame.Height);
            BarcodeContainer.Frame = new CGRect(screenCenterX - barcodeContainerFrame.Width + screenWidth, barcodeContainerFrame.Y, barcodeContainerFrame.Width, barcodeContainerFrame.Height);

            var duration = 0.666;
            UIView.Animate(duration, () =>
            {
                BarcodeMenu.Frame = new CGRect(screenCenterX - barcodeMenuFrame.Width * 0.5, barcodeMenuFrame.Y, barcodeMenuFrame.Width, barcodeMenuFrame.Height);
                BarcodeContainer.Frame = new CGRect(screenCenterX - barcodeContainerFrame.Width * 0.5, barcodeContainerFrame.Y, barcodeContainerFrame.Width, barcodeContainerFrame.Height);
            });
        }

        /// <summary>
        /// Slide send receive buttons out to left or right
        /// </summary>
        private void AnimateSlideSendReceiveButtonsOut(bool toLeft)
        {
            /*
            if (!barcodeIsLoaded)
            {
                AnimateSlideBarcodeToCenter();
            }
            */
            var screenWidth = UIScreen.MainScreen.Bounds.Width;

            var translationX = toLeft ? -screenWidth : screenWidth;
            var duration = 0.25;
            var sendButtonFrame = SendButton.Frame;
            var receiveButtonFrame = ReceiveButton.Frame;
            UIView.Animate(duration, () =>
            {
                SendButton.Frame = new CGRect(sendButtonFrame.X + translationX, sendButtonFrame.Y, sendButtonFrame.Width, sendButtonFrame.Height);
                ReceiveButton.Frame = new CGRect(receiveButtonFrame.X + translationX, receiveButtonFrame.Y, receiveButtonFrame.Width, receiveButtonFrame.Height);
            });
        }

        /// <summary>
        /// Slide barcode out to left or right
        /// </summary>
        private void AnimateSlideBarcodeOut(bool toLeft)
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Width;

            ViewModel.IsAnimatingBarcodeOut = true;
            ViewModel.IsReceiveButtonGreen = true;
            ViewModel.UploadTimerText = "";
            ViewModel.FileSize = "";

            SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
            ReceiveButton.Alpha = 0;

            if (toLeft)
                SendButton.Transform = CGAffineTransform.MakeTranslation(screenWidth, 0);

            var duration = 0.25;
            var barcodeTranslationX = toLeft ? -screenWidth : screenWidth;
            UIView.Animate(duration, () =>
            {
                SendButton.Transform = CGAffineTransform.MakeTranslation(0, 0);
                ReceiveButton.Alpha = 1;
                BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(barcodeTranslationX, 0);
                BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(barcodeTranslationX, 0);
            }, completion: () =>
            {
                ViewModel.DropViewUIState = DropViewState.SendReceiveButtonState;
                ViewModel.ResetUI();
            });

            /*
            sendButton.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .WithEndAction(new Java.Lang.Runnable(() => { ViewModel.DropViewUIState = DropViewState.SendReceiveButtonState; ViewModel.ResetUI(); }))
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
            */
        }

        /// <summary>
        /// Return barcode to center when user cancels a dismiss-slide action
        /// </summary>
        private void AnimateSlideBarcodeToCenter()
        {
            ViewModel.IsBarcodeVisible = true;

            var duration = 0.5;
            UIView.Animate(duration, () =>
            {
                BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
                BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(0, 0);
            });

            /*
            barcodeContainer.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
            barcodeMenu.Animate()
                .TranslationX(0)
                .SetDuration(duration)
                .Start();
            */
        }

        /// <summary>
        /// Slide the send receive buttons to screen center when user cancels swipe back to barcode action
        /// </summary>
        private void AnimateSlideSendReceiveCenter()
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Width;
            var screenCenterX = screenWidth * 0.5;

            var sendReceiveButtonsContainerFrame = SendReceiveButtonsContainer.Frame;
            var duration = 0.5;
            UIView.Animate(duration, () =>
            {
                SendReceiveButtonsContainer.Frame = new CGRect(screenCenterX - sendReceiveButtonsContainerFrame.Width * 0.5, sendReceiveButtonsContainerFrame.Y, sendReceiveButtonsContainerFrame.Width, sendReceiveButtonsContainerFrame.Height);
            });
        }

        /*
        /// <summary>
        /// Intercept touch events for the whole screen to handle swipe gestures
        /// </summary>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (!ViewModel.FirstFileUploaded || //don't allow swipe before first file is uploaded
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
                        sendReceiveButtonsContainer.TranslationX = sendReceiveButtonsContainerStartX + deltaX;
                    }

                    break;
            }

            return base.DispatchTouchEvent(e);
        }
        */

        private void SetupGestureListener()
        {
            var touchInterceptor = new TouchInterceptor();
            touchInterceptor.TouchDown += (s, e) =>
            {
                if (IgnoreSwipes()) return;

                Console.WriteLine($"TouchDown: ({e.X}, {e.Y})");

                isPressed = true;

                tapStartX = e.X;

                barcodeStartX = BarcodeContainer.Transform.x0;//.TranslationX;
                sendReceiveButtonsContainerStartX = SendReceiveButtonsContainer.Transform.x0;//.TranslationX;
            };

            touchInterceptor.TouchUp += (s, e) =>
            {
                if (IgnoreSwipes()) return;

                Console.WriteLine($"TouchUp: ({e.X}, {e.Y})");

                if (!isPressed) return;

                isPressed = false;

                if (!ViewModel.IsBarcodeVisible)
                {
                    //send & receive buttons are visible

                    if (SendReceiveButtonsContainer.Transform.x0/*.TranslationX*/ <= -swipeMarginX)
                        SetBarcodeCodeUiState();
                    else
                        AnimateSlideSendReceiveCenter();
                }
                else if (!ViewModel.IsAnimatingBarcodeOut)
                {
                    //barcode is visible

                    if (BarcodeContainer.Transform.x0/*.TranslationX*/ >= swipeMarginX)
                        SetSendReceiveButtonUiState();
                    else
                        AnimateSlideBarcodeToCenter();
                }
            };

            touchInterceptor.TouchMove += (s, e) =>
            {
                if (IgnoreSwipes()) return;

                Console.WriteLine($"TouchMove: ({e.X}, {e.Y})");

                if (!isPressed) return;

                var tapEndX = e.X;
                var deltaX = tapEndX - tapStartX;

                if (ViewModel.IsBarcodeVisible)
                {
                    var translationX = barcodeStartX + deltaX;
                    BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                    BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                }
                else
                {
                    var translationX = sendReceiveButtonsContainerStartX + deltaX;
                    SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                }
            };

            View.AddGestureRecognizer(touchInterceptor);
        }

        private bool IgnoreSwipes()
        {
            return !ViewModel.FirstFileUploaded || //don't allow swipe before first file is uploaded
                ViewModel.IsUploading || //don't allow swipe while file is uploading
                ViewModel.DropViewUIState == DropViewState.ConfirmFilesState; //don't allow swipe on confirm file UI state
        }
    }
}

