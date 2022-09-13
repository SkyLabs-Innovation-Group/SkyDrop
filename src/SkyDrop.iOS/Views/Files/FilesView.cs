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
        private UIBarButtonItem layoutToggleButton, deleteButton, moveButton, selectAllButton, saveUnzippedFilesButton;
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
            selectAllButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_select_all") };
            selectAllButton.Clicked += (s, e) => ViewModel.SelectAllCommand.Execute();
            saveUnzippedFilesButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_download") };
            saveUnzippedFilesButton.Clicked += (s, e) => ViewModel.SaveSelectedUnzippedFilesCommand.Execute();
            deleteButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_bin") };
            deleteButton.Clicked += (s, e) => ViewModel.DeleteFileCommand.Execute();
            moveButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_folder_move") };
            moveButton.Clicked += (s, e) => ViewModel.MoveFileCommand.Execute();
            layoutToggleButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_folder_add") };
            layoutToggleButton.Clicked += (s, e) => RightButtonTapped();
            NavigationItem.RightBarButtonItem = layoutToggleButton;
            NavigationItem.RightBarButtonItem.TintColor = UIColor.White;
            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.View.BackgroundColor = UIColor.Clear;
            NavigationController.NavigationBar.Translucent = true;
            NavigationController.NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
            NavigationController.NavigationBar.ShadowImage = new UIImage();

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
            set.Bind(this).For(t => t.ShowHideFileOptionsButtons).To(vm => vm.IsSelectionActive);
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

        public bool ShowHideFileOptionsButtons
        {
            get => false;
            set
            {
                if (value)
                {
                    //user is selecting something

                    if (ViewModel.IsFoldersVisible)
                        NavigationItem.RightBarButtonItems = new[] { deleteButton }; //show folder options buttons
                    else if (ViewModel.IsUnzippedFilesMode)
                        NavigationItem.RightBarButtonItems = new[] { saveUnzippedFilesButton, selectAllButton }; //show unzipped file options buttons
                    else
                        NavigationItem.RightBarButtonItems = new[] { moveButton, deleteButton }; //show file options buttons
                }
                else
                {
                    //no selection

                    if (ViewModel.IsUnzippedFilesMode)
                        NavigationItem.RightBarButtonItems = new[] { layoutToggleButton, selectAllButton };
                    else
                        NavigationItem.RightBarButtonItems = new[] { layoutToggleButton }; //show add folder / layout toggle button
                }
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
            //if folders are visible, show add folder icon
            //otherwise show files layout toggle icon
            var iconName = showFolders ? "ic_folder_add" :
                (ViewModel.LayoutType == FileLayoutType.List) ? "ic_grid" : "ic_list";
            layoutToggleButton.Image = UIImage.FromBundle(iconName);
        }
    }
}

