using System;
using System.Collections;
using Acr.UserDialogs;
using CoreGraphics;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FileExplorerView : UIView
    {
        public static readonly NSString Key = new NSString("FileExplorerView");
        public static readonly UINib Nib;
        private MvxCollectionViewSource collectionViewSource;

        private MvxSimpleTableViewSource tableViewSource;

        static FileExplorerView()
        {
            Nib = UINib.FromName("FileExplorerView", NSBundle.MainBundle);
        }

        protected FileExplorerView(IntPtr handle) : base(handle)
        {
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

        public static FileExplorerView CreateView()
        {
            return (FileExplorerView)Nib.Instantiate(null, null)[0];
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            BackgroundColor = Colors.DarkGrey.ToNative();
            FilesCollectionView.BackgroundColor = Colors.DarkGrey.ToNative();
            FilesTableView.BackgroundColor = Colors.DarkGrey.ToNative();
            FilesTableView.AllowsSelection = false;
            FilesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

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
            private const int HorizontalMargins = 16;
            private nfloat ItemWidth => (UIScreen.MainScreen.Bounds.Width - HorizontalMargins) / 2;

            public override CGSize ItemSize
            {
                get => new CGSize(ItemWidth, ItemWidth);
                set => base.ItemSize = value;
            }

            public override nfloat MinimumInteritemSpacing => 0;
            public override nfloat MinimumLineSpacing => 0;
        }
    }
}