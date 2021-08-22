using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using SkyDrop.iOS.Bindings;
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
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.iOS.Views.Drop
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class DropView : MvxViewController<DropViewModel>
    {
        private const int swipeMarginX = 20;
        private bool isPressed;
        private nfloat tapStartX, barcodeStartX, sendReceiveButtonsContainerStartX;
        private nfloat screenWidth => UIScreen.MainScreen.Bounds.Width;

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

        private void NavigateToSettings(object sender, EventArgs e)
        {
            ViewModel.NavigateToSettings().GetAwaiter().GetResult();
        }

        public override void ViewDidLoad()
        {
            try
            {
                base.ViewDidLoad();

                var settingsButton = new UIBarButtonItem()
                {
                    Image = UIImage.FromBundle("ic_settings")
                };
                settingsButton.Clicked += NavigateToSettings;

                NavigationItem.RightBarButtonItem = settingsButton;
                NavigationItem.RightBarButtonItem.TintColor = UIColor.White;

                ViewModel.SlideSendButtonToCenterCommand = new MvxCommand(AnimateSlideSendButton);
                ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;
                ViewModel.ResetUIStateCommand = new MvxCommand(SetSendReceiveButtonUiState);
                ViewModel.UpdateNavDotsCommand = new MvxCommand(() => UpdateNavDots());

                SetupGestureListener();
                SetupNavDots();

                //setup nav bar
                NavigationController.NavigationBar.BarTintColor = Colors.GradientDark.ToNative();
                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes()
                {
                    ForegroundColor = Colors.LightGrey.ToNative()
                };

                View.BackgroundColor = Colors.DarkGrey.ToNative();

                CancelButton.BackgroundColor = Colors.GradientOcean.ToNative();
                CancelButton.Layer.CornerRadius = 32;

                SendButton.BackgroundColor = Colors.Primary.ToNative();
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
                FilePreviewCollectionView.RegisterNibForCell(FilePreviewCollectionViewCell.Nib, FilePreviewCollectionViewCell.Key);
                set.Bind(filePreviewSource).For(s => s.ItemsSource).To(vm => vm.StagedFiles);
                set.Bind(FilePreviewCollectionView).For("Visible").To(vm => vm.IsStagedFilesVisible);

                set.Bind(SendButton).For("Tap").To(vm => vm.SendCommand);
                set.Bind(ReceiveButton).For("Tap").To(vm => vm.ReceiveCommand);

                set.Bind(CopyLinkButton).For("Tap").To(vm => vm.CopyLinkCommand);
                set.Bind(OpenButton).For("Tap").To(vm => vm.OpenFileInBrowserCommand);
                set.Bind(ShareButton).For("Tap").To(vm => vm.ShareLinkCommand);

                set.Bind(this).For(th => th.Title).To(vm => vm.Title);

                set.Bind(BarcodeMenu).For("Visible").To(vm => vm.IsBarcodeVisible);
                set.Bind(BarcodeContainer).For("Visible").To(vm => vm.IsBarcodeVisible);

                set.Bind(ActivityIndicator).For("Visible").To(vm => vm.IsUploading);
                set.Bind(SendIcon).For(v => v.Hidden).To(vm => vm.IsUploading);

                set.Bind(SendLabel).To(vm => vm.SendButtonLabel);

                set.Bind(FileSizeLabel).To(vm => vm.FileSize);

                set.Bind(ProgressFillArea).For("Visible").To(vm => vm.IsUploading);
                set.Bind(ProgressFillArea).For(ProgressFillHeightBinding.Name).To(vm => vm.UploadProgress);

                set.Bind(CancelButton).For("Visible").To(vm => vm.IsStagedFilesVisible);
                set.Bind(CancelButton).For("Tap").To(vm => vm.CancelUploadCommand);

                set.Bind(LeftNavDot).For("Visible").To(vm => vm.NavDotsVisible);
                set.Bind(RightNavDot).For("Visible").To(vm => vm.NavDotsVisible);

                set.Apply();
            }
            catch(Exception e)
            {
                ViewModel.Log.Exception(e);
            }
        }

        private void SetupNavDots()
        {
            LeftNavDot.Alpha = NavDotsMaxAlpha;
            RightNavDot.Alpha = NavDotsMinAlpha;
        }

        /// <summary>
        /// Change alpha of navigation dots display to reflect new UI state
        /// </summary>
        private void UpdateNavDots()
        {
            var duration = 0.25;

            switch (ViewModel.DropViewUIState)
            {
                case DropViewState.SendReceiveButtonState:
                case DropViewState.ConfirmFilesState:
                    UIView.Animate(duration, () =>
                    {
                        LeftNavDot.Alpha = NavDotsMaxAlpha;
                        RightNavDot.Alpha = NavDotsMinAlpha;
                    });
                    
                    break;
                case DropViewState.QRCodeState:
                    UIView.Animate(duration, () =>
                    {
                        LeftNavDot.Alpha = NavDotsMinAlpha;
                        RightNavDot.Alpha = NavDotsMaxAlpha;
                    });

                    break;
            }
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            try
            {
                SetBarcodeCodeUiState(isSlow: true);
                var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileFullUrl, (int)BarcodeImage.Frame.Width, (int)BarcodeImage.Frame.Height);
                var image = await iOSUtil.BitMatrixToImage(matrix);
                BarcodeImage.Image = image;
                ViewModel.BarcodeIsLoaded = true;
            }
            catch (Exception ex)
            {
                ViewModel.Log.Error("Error in ShowBarcode(): ");
                ViewModel.Log.Exception(ex);
            }
        }

        /// <summary>
        /// Return to the initial UI state
        /// </summary>
        private void SetSendReceiveButtonUiState()
        {
            //DropViewUIState gets changed at the end of the animation 
            //that is to fix an issue with CheckUserIsSwiping() on barcode menu buttons
            AnimateSlideBarcodeOut();
        }

        /// <summary>
        /// Show the QR code UI state
        /// </summary>
        private void SetBarcodeCodeUiState(bool isSlow)
        {
            ViewModel.DropViewUIState = DropViewState.QRCodeState;

            ViewModel.IsBarcodeVisible = true;
            
            ViewModel.Log.Trace("Sliding in QR code view");

            AnimateSlideBarcodeIn(isSlow);
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

            UIView.Animate(1, () =>
            {
                SendButton.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                ReceiveButton.Alpha = 0;
            });
        }

        /// <summary>
        /// Slide in the QR code from the left or right
        /// </summary>
        private void AnimateSlideBarcodeIn(bool isSlow = false)
        {
            var screenCenterX = screenWidth * 0.5;

            var barcodeTranslationX = screenWidth;

            BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(screenWidth, 0);
            BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(screenWidth, 0);

            var duration = isSlow ? 0.666 : 0.25;
            UIView.Animate(duration, () =>
            {
                BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(0, 0);
                BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
            });
        }

        /// <summary>
        /// Slide send receive buttons out to left or right
        /// </summary>
        private void AnimateSlideSendReceiveButtonsOut(bool toLeft)
        {
            var translationX = toLeft ? -screenWidth : screenWidth;
            var duration = 0.25;
            UIView.Animate(duration, () =>
            {
                SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                SendButton.Transform = CGAffineTransform.MakeTranslation(0, 0);
            });
        }

        /// <summary>
        /// Slide barcode out, slide send receive buttons in
        /// </summary>
        private void AnimateSlideBarcodeOut()
        {
            ViewModel.IsAnimatingBarcodeOut = true;
            ViewModel.IsReceiveButtonGreen = true;
            ViewModel.UploadTimerText = "";
            ViewModel.FileSize = "";
            ReceiveButton.Alpha = 0;

            var duration = 0.25;
            var barcodeTranslationX = screenWidth;
            UIView.Animate(duration, () =>
            {
                //slide barcode out
                BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(barcodeTranslationX, 0);
                BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(barcodeTranslationX, 0);

                //slide send receive buttons in
                ReceiveButton.Alpha = 1;
                SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
                SendButton.Transform = CGAffineTransform.MakeTranslation(0, 0);

            }, completion: () =>
            {
                ViewModel.DropViewUIState = DropViewState.SendReceiveButtonState;
                ViewModel.ResetUI();
            });
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
        }

        /// <summary>
        /// Slide the send receive buttons to screen center when user cancels swipe back to barcode action
        /// </summary>
        private void AnimateSlideSendReceiveCenter()
        {
            var screenCenterX = screenWidth * 0.5;

            var sendReceiveButtonsContainerFrame = SendReceiveButtonsContainer.Frame;
            var duration = 0.5;
            UIView.Animate(duration, () =>
            {
                SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
            });
        }

        private void SetupGestureListener()
        {
            var touchInterceptor = new TouchInterceptor();
            touchInterceptor.TouchDown += (s, e) =>
            {
                if (IgnoreSwipes()) return;

                isPressed = true;

                tapStartX = e.X;

                barcodeStartX = BarcodeContainer.Transform.x0;
                sendReceiveButtonsContainerStartX = SendReceiveButtonsContainer.Transform.x0;
            };

            touchInterceptor.TouchUp += (s, e) =>
            {
                if (IgnoreSwipes()) return;

                if (!isPressed) return;

                isPressed = false;

                if (ViewModel.DropViewUIState == DropViewState.SendReceiveButtonState)
                {
                    //send & receive buttons are visible

                    if (SendReceiveButtonsContainer.Transform.x0 <= -swipeMarginX)
                        SetBarcodeCodeUiState(isSlow: false);
                    else
                        AnimateSlideSendReceiveCenter();
                }
                else if (!ViewModel.IsAnimatingBarcodeOut)
                {
                    //barcode is visible

                    if (BarcodeContainer.Transform.x0 >= swipeMarginX)
                        SetSendReceiveButtonUiState();
                    else
                        AnimateSlideBarcodeToCenter();
                }
            };

            touchInterceptor.TouchMove += (s, e) =>
            {
                if (IgnoreSwipes()) return;

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

