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

namespace SkyDrop.iOS.Views.Drop
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class DropView : MvxViewController<DropViewModel>
    {
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

                set.Apply();
            }
            catch(Exception e)
            {
                ViewModel.Log.Exception(e);
            }
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
    }
}

