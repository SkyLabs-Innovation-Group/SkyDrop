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
    public partial class FilesView : MvxViewController<FilesViewModel>
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
            layoutToggleButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_list") };
            layoutToggleButton.Clicked += (s, e) => ToggleViewLayout();
            NavigationItem.RightBarButtonItem = layoutToggleButton;
            NavigationItem.RightBarButtonItem.TintColor = UIColor.White;
            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.View.BackgroundColor = UIColor.Clear;
            NavigationController.NavigationBar.Translucent = true;
            NavigationController.NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
            NavigationController.NavigationBar.ShadowImage = new UIImage();

            var set = CreateBindingSet();
            set.Bind(fileExplorerView).For(f => f.ItemsSource).To(vm => vm.SkyFiles);
            set.Bind(fileExplorerView).For(t => t.CollectionViewAndTableViewVisibility).To(vm => vm.LayoutType);
            set.Bind(ActivityIndicatorContainer).For("Visible").To(vm => vm.IsLoading);
            set.Bind(LoadingLabel).To(vm => vm.LoadingLabelText);
            set.Apply();
        }

        private void ToggleViewLayout()
        {
            var newLayoutType = ViewModel.LayoutType == FileLayoutType.Grid ? FileLayoutType.List : FileLayoutType.Grid;
            ViewModel.LayoutType = newLayoutType;
            layoutToggleButton.Image = newLayoutType == FileLayoutType.List ? UIImage.FromBundle("ic_grid") : UIImage.FromBundle("ic_list");
        }
    }
}

