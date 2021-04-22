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
using UIKit;
using ZXing.Mobile;
using ZXing.Rendering;
using static SkyDrop.Core.ViewModels.Main.DropViewModel;

namespace SkyDrop.iOS.Views.Drop
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class DropView : MvxViewController<DropViewModel>
    {
        public DropView() : base("DropView", null)
        {
        }

        public void SetBarcodeCodeUiState()
        {
            ViewModel.DropViewUIState = DropViewState.QRCodeState;

            ViewModel.IsBarcodeVisible = true;

            AnimateSlideBarcodeIn(fromLeft: false);
            AnimateSlideSendReceiveButtonsOut(toLeft: true);
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

                //setup nav bar
                NavigationController.NavigationBar.BarTintColor = Colors.GradientDark.ToNative();
                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes()
                {
                    ForegroundColor = Colors.LightGrey.ToNative()
                };

                View.BackgroundColor = Colors.DarkGrey.ToNative();

                SendButton.BackgroundColor = Colors.GradientGreen.ToNative();
                ReceiveButton.BackgroundColor = Colors.NewBlue.ToNative();
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
    }
}

