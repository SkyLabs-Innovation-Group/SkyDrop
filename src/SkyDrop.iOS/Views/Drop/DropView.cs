using System;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using SkyDrop.Core.Converters;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.iOS.Bindings;
using SkyDrop.iOS.Common;
using SkyDrop.iOS.Converters;
using SkyDrop.iOS.Styles;
using UIKit;
using UserNotifications;
using static SkyDrop.Core.Utility.Util;
using static SkyDrop.Core.ViewModels.Main.DropViewModel;

namespace SkyDrop.iOS.Views.Drop
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class DropView : BaseViewController<DropViewModel>
    {
        private const int SwipeMarginX = 20;
        private const string DropUploadNotifRequestId = "drop_upload_notification_id";
        private HomeMenuAnimator homeMenuAnimator;
        private bool isPressed, didInit;
        private nfloat tapStartX, barcodeStartX, sendReceiveButtonsContainerStartX;
        private UILabel titleLabel;

        public DropView() : base("DropView", null)
        {
        }

        private nfloat ScreenWidth => UIScreen.MainScreen.Bounds.Width;

        public string EncryptIconType
        {
            get => "";
            set
            {
                var icon = value == new AnyoneWithTheLinkItem().Name ? "ic_world" : "ic_key";
                EncryptIcon.Image = UIImage.FromBundle(icon);
            }
        }

        public override void ViewDidLoad()
        {
            try
            {
                base.ViewDidLoad();

                ViewModel.SlideSendButtonToCenterCommand = new MvxCommand(AnimateSlideSendButton);
                ViewModel.SlideReceiveButtonToCenterCommand = new MvxCommand(AnimateSlideReceiveButton);
                ViewModel.GenerateBarcodeAsyncFunc = t => ShowBarcode(t);
                ViewModel.ResetUiStateCommand = new MvxCommand(SetSendReceiveButtonUiState);
                ViewModel.UpdateNavDotsCommand = new MvxCommand(() => UpdateNavDots());
                ViewModel.UploadStartedNotificationCommand =
                    new MvxAsyncCommand(async () => await ShowUploadStartedNotification());
                ;
                ViewModel.UploadFinishedNotificationCommand =
                    new MvxCommand<FileUploadResult>(result => ShowUploadFinishedNotification(result));
                ViewModel.UpdateNotificationProgressCommand =
                    new MvxCommand<double>(progress => UpdateUploadNotificationProgress(progress));
                ViewModel.IosSelectFileCommand = new MvxCommand(() =>
                {
                    var successAction = new Action<string>(path => ViewModel.IosStageImage(path));
                    var failAction = new Action(() => ViewModel.IosImagePickerFailed());
                    ImageSelectionHelper.SelectMultiplePhoto(successAction, failAction);
                });

                var fileSystemService = Mvx.IoCProvider.Resolve<IFileSystemService>();
                fileSystemService.DownloadsFolderPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                fileSystemService.CacheFolderPath = Path.GetTempPath();

                SetupGestureListener();
                SetupNavDots();

                //setup nav bar
                NavigationController.NavigationBar.BarTintColor = Colors.GradientDark.ToNative();
                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
                {
                    ForegroundColor = Colors.White.ToNative()
                };

                MakeTitleTruncateFromMiddle();

                View.BackgroundColor = Colors.DarkGrey.ToNative();
                BarcodeContainer.BackgroundColor =
                    Colors.MidGrey.ToNative(); //so that preview image fades in from dark color

                CancelButton.BackgroundColor = Colors.MidGrey.ToNative();
                CancelButton.Layer.CornerRadius = 8;
                CancelIcon.TintColor = Colors.Red.ToNative();
                CancelLabel.TextColor = Colors.Red.ToNative();

                SendButton.StyleButton(Colors.Primary, true);
                ReceiveButton.StyleButton(Colors.GradientOcean, true);

                //QR menu
                CopyLinkButton.StyleButton(Colors.Primary);
                OpenButton.StyleButton(Colors.GradientGreen);
                DownloadButton.StyleButton(Colors.GradientTurqouise);
                ShareButton.StyleButton(Colors.GradientOcean);

                //home menu
                SkyDriveButton.StyleButton(Colors.Primary, true);
                PortalsButton.StyleButton(Colors.GradientGreen, true);
                ContactsButton.StyleButton(Colors.GradientTurqouise, true);
                SettingsButton.StyleButton(Colors.GradientOcean, true);

                MiniMenuContainer.BackgroundColor = Colors.MidGrey.ToNative();

                ProgressFillArea.BackgroundColor = Colors.Primary.ToNative();
                ProgressFillArea.Layer.CornerRadius = 8;

                BarcodeContainer.Layer.CornerRadius = 8;
                BarcodeContainer.ClipsToBounds = true;

                UrlLabelContainer.Layer.CornerRadius = 8;
                UrlLabelContainer.BackgroundColor = Colors.MidGrey.ToNative();

                ShowBarcodeButton.Layer.CornerRadius = 3;
                ShowBarcodeButton.BackgroundColor = Colors.MidGrey.ToNative().ColorWithAlpha(0.5f);

                ShowPreviewIcon.TintColor = UIColor.FromWhiteAlpha(0.8f, 1);

                FileTypeIcon.TintColor = Colors.LightGrey.ToNative();

                EncryptButton.BackgroundColor = Colors.Primary.ToNative();

                BindViews();
            }
            catch (Exception e)
            {
                ViewModel.Log.Exception(e);
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (didInit)
                return;

            //initialize animation container
            var animationContainer = new UIView { UserInteractionEnabled = false };
            View.LayoutInsideWithFrame(animationContainer);
            homeMenuAnimator = new HomeMenuAnimator(HomeMenuSkyDriveIcon, HomeMenuPortalsIcon, HomeMenuContactsIcon,
                HomeMenuSettingsIcon,
                MiniMenuSkyDriveIcon, MiniMenuPortalsIcon, MiniMenuContactsIcon, MiniMenuSettingsIcon,
                animationContainer);

            didInit = true;
        }

        private void BindViews()
        {
            var set = CreateBindingSet();

            //send & receive buttons
            set.Bind(SendButton).For("Tap").To(vm => vm.SendCommand);
            set.Bind(ReceiveButton).For("Tap").To(vm => vm.ReceiveCommand);
            set.Bind(SendActivityIndicator).For("Visible").To(vm => vm.IsUploading);
            set.Bind(ReceiveActivityIndicator).For("Visible").To(vm => vm.IsReceivingFile);
            set.Bind(SendIcon).For(v => v.Hidden).To(vm => vm.IsUploading);
            set.Bind(ReceiveIcon).For(v => v.Hidden).To(vm => vm.IsReceivingFile);
            set.Bind(SendLabel).To(vm => vm.SendButtonLabel);
            set.Bind(ReceiveLabel).To(vm => vm.ReceiveButtonLabel);
            set.Bind(FileSizeLabel).To(vm => vm.FileSize);
            set.Bind(ProgressFillArea).For("Visible").To(vm => vm.IsUploading);
            set.Bind(ProgressFillArea).For(ProgressFillHeightBinding.Name).To(vm => vm.UploadProgress);

            //home menu
            set.Bind(SkyDriveButton).For("Tap").To(vm => vm.MenuSkyDriveCommand);
            set.Bind(PortalsButton).For("Tap").To(vm => vm.MenuPortalsCommand);
            set.Bind(ContactsButton).For("Tap").To(vm => vm.MenuContactsCommand);
            set.Bind(SettingsButton).For("Tap").To(vm => vm.MenuSettingsCommand);

            //mini menu
            set.Bind(MiniMenuContainer).For("Visible").To(vm => vm.IsStagedFilesVisible);
            set.Bind(MiniMenuSkyDriveButton).For("Tap").To(vm => vm.MenuSkyDriveCommand);
            set.Bind(MiniMenuPortalsButton).For("Tap").To(vm => vm.MenuPortalsCommand);
            set.Bind(MiniMenuContactsButton).For("Tap").To(vm => vm.MenuContactsCommand);
            set.Bind(MiniMenuSettingsButton).For("Tap").To(vm => vm.MenuSettingsCommand);

            //QR menu
            set.Bind(CopyLinkButton).For("Tap").To(vm => vm.CopyLinkCommand);
            set.Bind(OpenButton).For("Tap").To(vm => vm.OpenFileInBrowserCommand);
            set.Bind(ShareButton).For("Tap").To(vm => vm.ShareLinkCommand);
            set.Bind(DownloadButton).For("Tap").To(vm => vm.DownloadFileCommand);
            set.Bind(DownloadButtonActivityIndicator).For("Visible").To(vm => vm.IsDownloadingFile);
            set.Bind(DownloadButtonIcon).For(t => t.Hidden).To(vm => vm.IsDownloadingFile);
            set.Bind(SaveFileLabel).For(t => t.Text).To(vm => vm.SaveButtonText);
            set.Bind(DownloadButtonIcon).For(a => a.ImagePath).To(vm => vm.IsFocusedFileAnArchive)
                .WithConversion(new SaveUnzipIconConverter());
            set.Bind(BarcodeMenu).For("Visible").To(vm => vm.IsBarcodeVisible);

            //barcode view
            set.Bind(BarcodeContainer).For("Visible").To(vm => vm.IsBarcodeVisible);
            set.Bind(PreviewImage).For("Visible").To(vm => vm.IsPreviewImageVisible);
            set.Bind(BarcodeImage).For(b => b.Hidden).To(vm => vm.IsPreviewImageVisible);
            set.Bind(UrlLabel).To(vm => vm.FocusedFileUrl);

            //icon behind preview image, to show while preview is loading
            set.Bind(FileTypeIcon).For(FileCategoryIconBinding.Name).To(vm => vm.FocusedFile.Filename);

            //for barcode / preview toggle
            set.Bind(ShowBarcodeButton).For("Visible").To(vm => vm.IsShowBarcodeButtonVisible);
            set.Bind(ShowBarcodeButton).For("Tap").To(vm => vm.ShowBarcodeCommand);
            set.Bind(ShowPreviewButton).For("Visible").To(vm => vm.IsShowPreviewButtonVisible);
            set.Bind(ShowPreviewButton).For("Tap").To(vm => vm.ShowPreviewImageCommand);
            set.Bind(PreviewImage).For(i => i.ImagePath).To(vm => vm.PreviewImageUrl);

            //encryption button
            set.Bind(EncryptButton).For("Tap").To(vm => vm.ChooseRecipientCommand);
            set.Bind(EncryptButton).For("Visible").To(vm => vm.IsStagedFilesVisible);
            set.Bind(EncryptButton).For("BackgroundColor").To(vm => vm.EncryptionButtonColor)
                .WithConversion(new NativeColorConverter());
            set.Bind(EncryptionLabel).To(vm => vm.EncryptionText);
            set.Bind(this).For(t => t.EncryptIconType).To(vm => vm.EncryptionText);

            set.Bind(CancelButton).For("Visible").To(vm => vm.IsStagedFilesVisible);
            set.Bind(CancelButton).For("Tap").To(vm => vm.CancelUploadCommand);

            //setup file preview collection view
            var filePreviewSource =
                new MvxCollectionViewSource(FilePreviewCollectionView, FilePreviewCollectionViewCell.Key);
            FilePreviewCollectionView.DataSource = filePreviewSource;
            FilePreviewCollectionView.RegisterNibForCell(FilePreviewCollectionViewCell.Nib,
                FilePreviewCollectionViewCell.Key);
            set.Bind(filePreviewSource).For(s => s.ItemsSource).To(vm => vm.StagedFiles);
            set.Bind(FilePreviewCollectionView).For("Visible").To(vm => vm.IsStagedFilesVisible);

            set.Bind(LeftNavDot).For("Visible").To(vm => vm.NavDotsVisible);
            set.Bind(RightNavDot).For("Visible").To(vm => vm.NavDotsVisible);

            set.Bind(titleLabel).To(vm => vm.Title);

            set.Apply();
        }

        private void MakeTitleTruncateFromMiddle()
        {
            titleLabel = new UILabel
            {
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.BoldSystemFontOfSize(17),
                Text = "SkyDrop",
                TextColor = Colors.LightGrey.ToNative(),
                LineBreakMode = UILineBreakMode.MiddleTruncation,

                //makes label auto resize after text changes
                Bounds = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 48),
                TranslatesAutoresizingMaskIntoConstraints = true
            };

            NavigationItem.TitleView = titleLabel;
        }

        private void UpdateUploadNotificationProgress(double progress)
        {
            var progressPercentage = (int)Math.Floor(progress * 100);

            var content = new UNMutableNotificationContent();
            content.Title = "Upload started";
            if (progress > 1.0)
                progressPercentage = 100;

            content.Body = $"{progressPercentage}% complete";

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var request = UNNotificationRequest.FromIdentifier(DropUploadNotifRequestId, content, trigger);
            UNUserNotificationCenter.Current.AddNotificationRequest(request, err => { });
        }

        private async Task ShowUploadStartedNotification()
        {
            var (granted, _) =
                await UNUserNotificationCenter.Current.RequestAuthorizationAsync(UNAuthorizationOptions.Alert |
                    UNAuthorizationOptions.Badge);

            if (!granted)
                // No notification permission
                return;

            var content = new UNMutableNotificationContent();
            content.Title = "Upload started";
            content.Body = "0% complete";
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

            var request = UNNotificationRequest.FromIdentifier(DropUploadNotifRequestId, content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, err =>
            {
                if (err != null)
                {
                    ViewModel.Log.Error($"{err.Description}");
                    ViewModel.Log.Error($"{err}");
                }
            });
        }

        private void ShowUploadFinishedNotification(FileUploadResult result)
        {
            var content = new UNMutableNotificationContent();

            switch (result)
            {
                case FileUploadResult.Success:
                    content.Title = "File published successfully (tap to view)";
                    break;
                case FileUploadResult.Fail:
                    content.Title = "Upload failed";
                    break;
                case FileUploadResult.Cancelled:
                    return;
            }

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

            var request = UNNotificationRequest.FromIdentifier(DropUploadNotifRequestId, content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, err => { });
        }

        private void SetupNavDots()
        {
            LeftNavDot.Alpha = NavDotsMaxAlpha;
            RightNavDot.Alpha = NavDotsMinAlpha;
        }

        /// <summary>
        ///     Change alpha of navigation dots display to reflect new UI state
        /// </summary>
        private void UpdateNavDots()
        {
            var duration = 0.25;

            switch (ViewModel.DropViewUiState)
            {
                case DropViewState.SendReceiveButtonState:
                case DropViewState.ConfirmFilesState:
                    UIView.Animate(duration, () =>
                    {
                        LeftNavDot.Alpha = NavDotsMaxAlpha;
                        RightNavDot.Alpha = NavDotsMinAlpha;
                    });

                    break;
                case DropViewState.QrCodeState:
                    UIView.Animate(duration, () =>
                    {
                        LeftNavDot.Alpha = NavDotsMinAlpha;
                        RightNavDot.Alpha = NavDotsMaxAlpha;
                    });

                    break;
            }
        }

        /// <summary>
        ///     Generate and display QR code
        /// </summary>
        private async Task ShowBarcode(string url)
        {
            try
            {
                SetBarcodeCodeUiState(true);

                var screenDensity = (int)UIScreen.MainScreen.Scale;
                var matrix = ViewModel.GenerateBarcode(url, (int)BarcodeImage.Frame.Width * screenDensity,
                    (int)BarcodeImage.Frame.Height * screenDensity);
                var image = await IOsUtil.BitMatrixToImage(matrix);
                BarcodeImage.Image = image;
                ViewModel.SwipeNavigationEnabled = true;
            }
            catch (Exception ex)
            {
                ViewModel.Log.Error("Error in ShowBarcode(): ");
                ViewModel.Log.Exception(ex);
            }
        }

        /// <summary>
        ///     Return to the initial UI state
        /// </summary>
        private void SetSendReceiveButtonUiState()
        {
            //DropViewUIState gets changed at the end of the animation 
            //that is to fix an issue with CheckUserIsSwiping() on barcode menu buttons
            AnimateSlideBarcodeOut();

            ViewModel.Title = "SkyDrop";

            ReceiveButton.Transform = CGAffineTransform.MakeTranslation(0, 0);
            SendButton.Alpha = 1;
        }

        /// <summary>
        ///     Show the QR code UI state
        /// </summary>
        private void SetBarcodeCodeUiState(bool isSlow)
        {
            ViewModel.DropViewUiState = DropViewState.QrCodeState;
            ViewModel.IsBarcodeVisible = true;
            ViewModel.Title = ViewModel.FocusedFile?.Filename;

            ViewModel.Log.Trace("Sliding in QR code view");

            AnimateSlideBarcodeIn(isSlow);
            AnimateSlideSendReceiveButtonsOut(true);
        }

        /// <summary>
        ///     Slide send button to center
        /// </summary>
        private void AnimateSlideSendButton()
        {
            var screenCenterX = UIScreen.MainScreen.Bounds.Width / 2;
            var sendButtonCenterX = SendButton
                .ConvertPointToView(new CGPoint(SendButton.Bounds.Width * 0.5, SendButton.Bounds.Height), null).X;
            var translationX = screenCenterX - sendButtonCenterX;

            var duration = 1;
            UIView.Animate(duration, () =>
            {
                SendButton.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                ReceiveButton.Alpha = 0;
            });

            UIView.Animate(duration / 3f, () => { HomeMenu.Alpha = 0; });

            MiniMenuContainer.Alpha = 0;
            UIView.Animate(duration / 3f, duration * 2f / 3f, UIViewAnimationOptions.CurveLinear,
                () => { MiniMenuContainer.Alpha = 1; }, null);

            homeMenuAnimator.AnimateShrink(duration / 3f, duration / 3f);
        }

        /// <summary>
        ///     Slide receive button to center
        /// </summary>
        private void AnimateSlideReceiveButton()
        {
            var screenCenterX = UIScreen.MainScreen.Bounds.Width / 2;

            var receiveButtonCenterX = ReceiveButton
                .ConvertPointToView(new CGPoint(ReceiveButton.Bounds.Width * 0.5, ReceiveButton.Bounds.Height), null).X;
            var translationX = screenCenterX - receiveButtonCenterX;

            UIView.Animate(1, () =>
            {
                ReceiveButton.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                SendButton.Alpha = 0;
                HomeMenu.Alpha = 0;
            });
        }

        /// <summary>
        ///     Slide in the QR code from the left or right
        /// </summary>
        private void AnimateSlideBarcodeIn(bool isSlow = false)
        {
            var screenCenterX = ScreenWidth * 0.5;
            var barcodeTranslationX = ScreenWidth;

            BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(ScreenWidth, 0);
            BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(ScreenWidth, 0);

            var duration = isSlow ? 0.666 : 0.25;
            UIView.Animate(duration, () =>
            {
                BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(0, 0);
                BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
            });
        }

        /// <summary>
        ///     Slide send receive buttons out to left or right
        /// </summary>
        private void AnimateSlideSendReceiveButtonsOut(bool toLeft)
        {
            var translationX = toLeft ? -ScreenWidth : ScreenWidth;
            var duration = 0.25;
            UIView.Animate(duration, () =>
            {
                SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                SendButton.Transform = CGAffineTransform.MakeTranslation(0, 0);
                ReceiveButton.Transform = CGAffineTransform.MakeTranslation(0, 0);
                HomeMenu.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
            });
        }

        /// <summary>
        ///     Slide barcode out, slide send receive buttons in
        /// </summary>
        private void AnimateSlideBarcodeOut()
        {
            ViewModel.IsAnimatingBarcodeOut = true;
            ViewModel.IsReceiveButtonGreen = true;
            ViewModel.UploadTimerText = "";
            ViewModel.FileSize = "";
            ReceiveButton.Alpha = 0;

            var duration = 0.25;
            var barcodeTranslationX = ScreenWidth;
            UIView.Animate(duration, () =>
            {
                //slide barcode out
                BarcodeContainer.Transform = CGAffineTransform.MakeTranslation(barcodeTranslationX, 0);
                BarcodeMenu.Transform = CGAffineTransform.MakeTranslation(barcodeTranslationX, 0);

                //slide send receive buttons in
                SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);

                SendButton.Alpha = 1;
                SendButton.Transform = CGAffineTransform.MakeTranslation(0, 0);

                ReceiveButton.Alpha = 1;
                ReceiveButton.Transform = CGAffineTransform.MakeTranslation(0, 0);

                HomeMenu.Alpha = 1;
                HomeMenu.Transform = CGAffineTransform.MakeTranslation(0, 0);

                MiniMenuContainer.Alpha = 0;
            }, () =>
            {
                ViewModel.DropViewUiState = DropViewState.SendReceiveButtonState;
                ViewModel.ResetUi();
            });
        }

        /// <summary>
        ///     Return barcode to center when user cancels a dismiss-slide action
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
        ///     Slide the send receive buttons to screen center when user cancels swipe back to barcode action
        /// </summary>
        private void AnimateSlideSendReceiveCenter()
        {
            var screenCenterX = ScreenWidth * 0.5;

            var sendReceiveButtonsContainerFrame = SendReceiveButtonsContainer.Frame;
            var duration = 0.5;
            UIView.Animate(duration, () =>
            {
                SendReceiveButtonsContainer.Transform = CGAffineTransform.MakeTranslation(0, 0);
                HomeMenu.Transform = CGAffineTransform.MakeTranslation(0, 0);
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

                if (ViewModel.DropViewUiState == DropViewState.SendReceiveButtonState)
                {
                    //send & receive buttons are visible

                    if (SendReceiveButtonsContainer.Transform.x0 <= -SwipeMarginX)
                        SetBarcodeCodeUiState(false);
                    else
                        AnimateSlideSendReceiveCenter();
                }
                else if (!ViewModel.IsAnimatingBarcodeOut)
                {
                    //barcode is visible

                    if (BarcodeContainer.Transform.x0 >= SwipeMarginX)
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
                    HomeMenu.Transform = CGAffineTransform.MakeTranslation(translationX, 0);
                }
            };

            View.AddGestureRecognizer(touchInterceptor);
        }

        private bool IgnoreSwipes()
        {
            return !ViewModel.SwipeNavigationEnabled || //don't allow swipe before first file is uploaded
                   ViewModel.IsUploading || //don't allow swipe while file is uploading
                   ViewModel.DropViewUiState ==
                   DropViewState.ConfirmFilesState; //don't allow swipe on confirm file UI state
        }
    }
}