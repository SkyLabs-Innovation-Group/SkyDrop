using System;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FilesView : MvxViewController<FilesViewModel>
    {
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

            var collectionViewSource = new MvxCollectionViewSource(FilesCollectionView, FileCollectionViewCell.Key);
            FilesCollectionView.RegisterNibForCell(FileCollectionViewCell.Nib, FileCollectionViewCell.Key);
            FilesCollectionView.Source = collectionViewSource;
            FilesCollectionView.CollectionViewLayout = new FilesCollectionViewLayout();

            var set = CreateBindingSet();
            set.Bind(collectionViewSource).For(f => f.ItemsSource).To(vm => vm.SkyFiles);
            set.Apply();
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

