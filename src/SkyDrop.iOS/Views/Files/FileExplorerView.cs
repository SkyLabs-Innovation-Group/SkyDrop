using System;
using System.Collections;
using Acr.UserDialogs;
using CoreGraphics;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;
using static SkyDrop.iOS.Views.Files.FilesView;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FileExplorerView : UIView
    {
        public static readonly NSString Key = new NSString("FileExplorerView");
        public static readonly UINib Nib;

        private MvxSimpleTableViewSource tableViewSource;
        private MvxCollectionViewSource collectionViewSource;

        static FileExplorerView()
        {
            Nib = UINib.FromName("FileExplorerView", NSBundle.MainBundle);
        }

        protected FileExplorerView(IntPtr handle) : base(handle)
        {
        }

        public static FileExplorerView CreateView()
        {
            return (FileExplorerView)Nib.Instantiate(null, null)[0];
        }

        public FileLayoutType CollectionViewAndTableViewVisibility
        {
            get => FileLayoutType.List;
            set
            {
                FilesCollectionView.Hidden = value == FileLayoutType.List;
                FilesTableView.Hidden = value == FileLayoutType.Grid;
            }
        }

        public IEnumerable ItemsSource
        {
            get => null;
            set
            {
                tableViewSource.ItemsSource = value;
                collectionViewSource.ItemsSource = value;
            }
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            BackgroundColor = Colors.DarkGrey.ToNative();
            FilesCollectionView.BackgroundColor = Colors.DarkGrey.ToNative();
            FilesTableView.BackgroundColor = Colors.DarkGrey.ToNative();
            FilesTableView.AllowsSelection = false;

            collectionViewSource = new MvxCollectionViewSource(FilesCollectionView, FileCollectionViewCell.Key);
            FilesCollectionView.RegisterNibForCell(FileCollectionViewCell.Nib, FileCollectionViewCell.Key);
            FilesCollectionView.Source = collectionViewSource;
            FilesCollectionView.CollectionViewLayout = new FilesCollectionViewLayout();

            tableViewSource = new MvxSimpleTableViewSource(FilesTableView, FileTableViewCell.Key);
            FilesTableView.RegisterNibForCellReuse(FileTableViewCell.Nib, FileTableViewCell.Key);
            FilesTableView.Source = tableViewSource;
        }

        private class FilesCollectionViewLayout : UICollectionViewFlowLayout
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

