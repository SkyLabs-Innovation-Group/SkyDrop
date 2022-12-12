using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.Converters;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using SkyDrop.iOS.Bindings;
using SkyDrop.iOS.Common;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    //cell for a grid file item in the gallery view
    public partial class FileCollectionViewCell : MvxCollectionViewCell
    {
        public static readonly NSString Key = new NSString("FileCollectionViewCell");
        public static readonly UINib Nib;

        static FileCollectionViewCell()
        {
            Nib = UINib.FromName("FileCollectionViewCell", NSBundle.MainBundle);
        }

        protected FileCollectionViewCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<FileCollectionViewCell, SkyFileDVM>();
                set.Bind(FileNameLabel).To(vm => vm.SkyFile.Filename);
                set.Bind(this).For(t => t.SkyFile).To(vm => vm.SkyFile);
                set.Bind(BottomPanel).For(i => i.BackgroundColor).To(vm => vm.FillColor).WithConversion("NativeColor");
                set.Bind(SelectedIndicatorView).For(i => i.BackgroundColor).To(vm => vm.SelectionIndicatorColor)
                    .WithConversion("NativeColor");
                set.Bind(SelectedIndicatorView).For("Visible").To(vm => vm.IsSelectionActive);
                set.Bind(SelectedIndicatorInnerView).For("Visible").To(vm => vm.IsSelected);
                set.Bind(ContentView).For("Tap").To(vm => vm.TapCommand);
                set.Bind(ContentView).For("LongPress").To(vm => vm.LongPressCommand);
                set.Bind(ActivityIndicatorContainer).For("Visible").To(vm => vm.IsLoading);
                set.Bind(ActivityIndicatorContainer).For(i => i.BackgroundColor).To(vm => vm.FillColor)
                    .WithConversion("NativeColor");
                set.Bind(IconImage).For(FileCategoryIconBinding.Name).To(vm => vm.SkyFile.Filename);
                set.Bind(IconImage).For("Visible").To(vm => vm.SkyFile.Filename)
                    .WithConversion(CanDisplayPreviewConverter.InvertName);
                set.Apply();
            });
        }

        public SkyFile SkyFile
        {
            get => new SkyFile();
            set
            {
                //clear previous preview image
                PreviewImage.ImagePath = null;

                //update preview image
                if (value.Filename.CanDisplayPreview())
                {
                    if (value.Skylink.IsNullOrEmpty())
                    {
                        //this is an "unzipped" file
                        iOSUtil.LoadLocalImagePreview(value.FullFilePath, PreviewImage);
                        return;
                    }

                    //this is a SkyFile
                    var url = value.GetSkylinkUrl();
                    PreviewImage.ImagePath = url;
                }
            }
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            ContainerView.BackgroundColor = Colors.DarkGrey.ToNative();
            InnerView.BackgroundColor = Colors.MidGrey.ToNative();
            BottomPanel.BackgroundColor = Colors.MidGrey.ToNative();

            InnerView.Layer.CornerRadius = 8;
            InnerView.Layer.MasksToBounds = true;
        }
    }
}