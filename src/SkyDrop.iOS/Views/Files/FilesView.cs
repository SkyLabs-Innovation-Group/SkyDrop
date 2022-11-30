using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.iOS.Common;
using SkyDrop.iOS.Styles;
using UIKit;
using static SkyDrop.iOS.Common.iOSUtil;

namespace SkyDrop.iOS.Views.Files
{
    [MvxChildPresentationAttribute]
    public partial class FilesView : BaseViewController<FilesViewModel>
    {
        private UIBarButtonItem layoutToggleButton, addFolderButton, deleteButton, moveButton, selectAllButton, saveFilesButton;
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
            ViewModel.UpdateTopBarButtonsCommand = new MvxCommand(UpdateTopBarButtons);
            selectAllButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_select_all") };
            selectAllButton.Clicked += (s, e) => ViewModel.SelectAllCommand.Execute();
            saveFilesButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_download") };
            saveFilesButton.Clicked += (s, e) => ViewModel.SaveSelectedUnzippedFilesCommand.Execute();
            addFolderButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_folder_add") };
            addFolderButton.Clicked += (s, e) => ViewModel.AddFolderCommand.Execute();
            deleteButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_bin") };
            deleteButton.Clicked += (s, e) => ViewModel.DeleteFileCommand.Execute();
            moveButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_folder_move") };
            moveButton.Clicked += (s, e) => ViewModel.MoveFileCommand.Execute();
            layoutToggleButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_folder_list") };
            layoutToggleButton.Clicked += (s, e) => ToggleViewLayout();
            NavigationItem.RightBarButtonItem = layoutToggleButton;
            NavigationItem.RightBarButtonItem.TintColor = UIColor.White;
            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.View.BackgroundColor = UIColor.Clear;
            UpdateTopBarButtons();

            AddBackButton(() => ViewModel.BackCommand.Execute());

            SaveArchiveButton.StyleButton(Colors.GradientTurqouise);
            ExtractArchiveButton.StyleButton(Colors.GradientGreen);

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
            set.Bind(SaveArchiveButton).For("Visible").To(vm => vm.IsUnzipError);
            set.Bind(ExtractArchiveButton).For("Visible").To(vm => vm.IsUnzipError);
            set.Bind(SaveArchiveButton).For("Tap").To(vm => vm.SaveArchiveCommand);
            set.Bind(ExtractArchiveButton).For("Tap").To(vm => vm.ExtractArchiveCommand);
            set.Bind(SaveArchiveSpinner).For("Visible").To(vm => vm.IsSavingArchive);
            set.Bind(ExtractArchiveSpinner).For("Visible").To(vm => vm.IsExtractingArchive);
            set.Bind(SaveArchiveIcon).For(v => v.Hidden).To(vm => vm.IsSavingArchive);
            set.Bind(ExtractArchiveIcon).For(v => v.Hidden).To(vm => vm.IsExtractingArchive);
            set.Bind(LoadingLabel).To(vm => vm.LoadingLabelText);
            set.Bind(ErrorDetailsLabel).To(vm => vm.ErrorDetailText);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();
        }

        private void UpdateTopBarButtons()
        {
            var buttons = new List<UIBarButtonItem>();

            if (ViewModel.IsSelectAllButtonVisible)
                buttons.Add(selectAllButton);

            if (ViewModel.IsLayoutButtonVisible)
                buttons.Add(layoutToggleButton);

            if (ViewModel.IsAddFolderButtonVisible)
                buttons.Add(addFolderButton);

            if (ViewModel.IsDeleteButtonVisible)
                buttons.Add(deleteButton);

            if (ViewModel.IsMoveButtonVisible)
                buttons.Add(moveButton);

            if (ViewModel.IsSaveButtonVisible)
                buttons.Add(saveFilesButton);

            NavigationItem.RightBarButtonItems = buttons.ToArray();
            UpdateLayoutButtonIcon();
        }

        private void ToggleViewLayout()
        {
            var newLayoutType = ViewModel.LayoutType == FileLayoutType.Grid ? FileLayoutType.List : FileLayoutType.Grid;
            ViewModel.LayoutType = newLayoutType;
            UpdateLayoutButtonIcon();
        }

        private void UpdateLayoutButtonIcon()
        {
            var iconName = ViewModel.LayoutType == FileLayoutType.List ? "ic_grid" : "ic_list";
            layoutToggleButton.Image = UIImage.FromBundle(iconName);
        }
    }
}

