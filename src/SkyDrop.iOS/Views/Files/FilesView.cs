using System;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    [MvxChildPresentationAttribute]
    public partial class FilesView : MvxViewController<FilesViewModel>
    {
        private UIBarButtonItem layoutToggleButton;

        public FilesView() : base("FilesView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Colors.DarkGrey.ToNative();
            FilesCollectionView.BackgroundColor = Colors.DarkGrey.ToNative();

            //setup nav bar
            NavigationController.NavigationBar.TintColor = UIColor.White;
            layoutToggleButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_menu") };
            layoutToggleButton.Clicked += (s, e) => ToggleViewLayout();
            NavigationItem.RightBarButtonItem = layoutToggleButton;
            NavigationItem.RightBarButtonItem.TintColor = UIColor.White;

            var collectionViewSource = new MvxCollectionViewSource(FilesCollectionView, FileCollectionViewCell.Key);
            FilesCollectionView.RegisterNibForCell(FileCollectionViewCell.Nib, FileCollectionViewCell.Key);
            FilesCollectionView.Source = collectionViewSource;
            FilesCollectionView.CollectionViewLayout = new FilesCollectionViewLayout();

            var set = CreateBindingSet();
            set.Bind(collectionViewSource).For(f => f.ItemsSource).To(vm => vm.SkyFiles);
            set.Bind(collectionViewSource).For(f => f.SelectionChangedCommand).To(vm => vm.FileSelectedCommand);
            set.Apply();
        }

        private void ToggleViewLayout()
        {
            var newLayoutType = ViewModel.LayoutType == FileLayoutType.Grid ? FileLayoutType.List : FileLayoutType.Grid;
            ViewModel.LayoutType = newLayoutType;
            layoutToggleButton.Image = newLayoutType == FileLayoutType.List ? UIImage.FromBundle("ic_cancel") : UIImage.FromBundle("ic_menu");
        }

        public class FilesCollectionViewLayout : UICollectionViewFlowLayout
        {
            private const int horizontalMargins = 16;
            private nfloat itemWidth => (UIScreen.MainScreen.Bounds.Width - horizontalMargins) / 2;

            public override CGSize ItemSize
            {
                get => new CGSize(itemWidth, itemWidth);
                set => base.ItemSize = value;
            }

            public override nfloat MinimumInteritemSpacing => 0;
            public override nfloat MinimumLineSpacing => 0;
        }
    }
}

