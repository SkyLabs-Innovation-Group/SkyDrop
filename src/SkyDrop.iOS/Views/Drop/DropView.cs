using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreGraphics;
using Foundation;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;
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
            //AnimateSlideSendReceiveButtonsOut(toLeft: true);
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
                NavigationController.Title = "SkyDrop";
                var barColor = Colors.GradientDeepBlue.ToNative();
                NavigationController.NavigationBar.BarTintColor = barColor;

                View.BackgroundColor = Colors.DarkGrey.ToNative();

                SendButton.BackgroundColor = Colors.Primary.ToNative();
                ReceiveButton.BackgroundColor = Colors.GradientTurqouise.ToNative();

                SendButton.Layer.CornerRadius = 8;
                ReceiveButton.Layer.CornerRadius = 8;

                var set = CreateBindingSet();

                //setup file preview collection view
                var filePreviewSource = new MvxCollectionViewSource(FilePreviewCollectionView, FilePreviewCollectionViewCell.Key);
                FilePreviewCollectionView.DataSource = filePreviewSource;
                FilePreviewCollectionView.RegisterClassForCell(typeof(FilePreviewCollectionViewCell), FilePreviewCollectionViewCell.Key);
                set.Bind(filePreviewSource).For(s => s.ItemsSource).To(vm => vm.StagedFiles);
                set.Bind(FilePreviewCollectionView).For("Visible").To(vm => vm.IsStagedFilesVisible);

                set.Bind(SendButton).For("Tap").To(vm => vm.SendCommand);
                set.Bind(ReceiveButton).For("Tap").To(vm => vm.ReceiveCommand);
                set.Bind(Title).To(vm => vm.Title);
                // set.Bind(NavigationController.nav).For(n => n.NavigationBar)

                set.Bind(BarcodeMenu).For("Visible").To(vm => vm.IsBarcodeVisible);
                set.Bind(BarcodeContainer).For("Visible").To(vm => vm.IsBarcodeVisible);

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
            /*
            var matrix = ViewModel.GenerateBarcode(ViewModel.SkyFileJson, barcodeImageView.Width, barcodeImageView.Height);
            var bitmap = await AndroidUtil.EncodeBarcode(matrix, barcodeImageView.Width, barcodeImageView.Height);
            barcodeImageView.SetImageBitmap(bitmap);
            barcodeIsLoaded = true;
            */
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

                //ReceiveButton.Alpha = 0;
            });
            /*
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
            */
        }
    }
}

