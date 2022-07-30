using System;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.iOS.Common;
using UIKit;
using static SkyDrop.iOS.Common.iOSUtil;

namespace SkyDrop.iOS.Views.Files
{
    [MvxChildPresentationAttribute]
    public partial class FilesView : BaseViewController<FilesViewModel>
    {
        private UIBarButtonItem layoutToggleButton;
        private FileExplorerView fileExplorerView;

        public FilesView() : base("FilesView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Colors.DarkGrey.ToNative();

            fileExplorerView = FileExplorerView.CreateView();
            FileExplorerHolder.LayoutInsideWithFrame(fileExplorerView);

            //setup nav bar
            layoutToggleButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_folder_add") };
            layoutToggleButton.Clicked += (s, e) => RightButtonTapped();
            NavigationItem.RightBarButtonItem = layoutToggleButton;
            NavigationItem.RightBarButtonItem.TintColor = UIColor.White;
            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.View.BackgroundColor = UIColor.Clear;
            NavigationController.NavigationBar.Translucent = true;
            NavigationController.NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
            NavigationController.NavigationBar.ShadowImage = new UIImage();


            //TODO: find some way to override back button behavior
            AddBackButton(() => ViewModel.BackCommand.Execute());

                

            var folderSource = new MvxSimpleTableViewSource(FoldersTableView, FolderCell.Key);
            FoldersTableView.Source = folderSource;
            FoldersTableView.RegisterNibForCellReuse(FolderCell.Nib, FolderCell.Key);
            FoldersTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            FoldersTableView.BackgroundColor = Colors.DarkGrey.ToNative();

            var set = CreateBindingSet();
            set.Bind(FoldersTableView).For("Visible").To(vm => vm.IsFoldersVisible);
            set.Bind(fileExplorerView).For(a => a.Hidden).To(vm => vm.IsFoldersVisible);
            set.Bind(folderSource).For(f => f.ItemsSource).To(vm => vm.Folders);
            set.Bind(fileExplorerView).For(f => f.ItemsSource).To(vm => vm.SkyFiles);
            set.Bind(fileExplorerView).For(t => t.CollectionViewAndTableViewVisibility).To(vm => vm.LayoutType);
            set.Bind(ActivityIndicatorContainer).For("Visible").To(vm => vm.IsLoadingLabelVisible);
            set.Bind(ActivityIndicator).For(a => a.Hidden).To(vm => vm.IsError);
            set.Bind(ErrorIcon).For("Visible").To(vm => vm.IsError);
            set.Bind(LoadingLabel).To(vm => vm.LoadingLabelText);
            set.Bind(this).For(t => t.ShowHideFolders).To(vm => vm.IsFoldersVisible);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();
        }

        public bool ShowHideFolders
        {
            get => false;
            set
            {
                UpdateButtonIcon(value);
            }
        }

        private void RightButtonTapped()
        {
            if (ViewModel.IsFoldersVisible)
                ViewModel.AddFolderCommand?.Execute();
            else
                ToggleViewLayout();
        }

        private void ToggleViewLayout()
        {
            var newLayoutType = ViewModel.LayoutType == FileLayoutType.Grid ? FileLayoutType.List : FileLayoutType.Grid;
            ViewModel.LayoutType = newLayoutType;
            UpdateButtonIcon(false);
        }

        private void UpdateButtonIcon(bool showFolders)
        {
            var iconName = showFolders ? "ic_folder_add" : ViewModel.LayoutType == FileLayoutType.List ? "ic_grid" : "ic_list";
            layoutToggleButton.Image = UIImage.FromBundle(iconName);
        }
    }
}

