﻿using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
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
                set.Apply();
            });
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

        public SkyFile SkyFile
        {
            get => new SkyFile();
            set
            {
                //update preview image
                if (Util.ExtensionMatches(value.Filename, ".jpeg", ".jpg", ".png"))
                {
                    var filePath = value?.GetSkylinkUrl();
                    PreviewImage.ImagePath = filePath;
                }
            }
        }
    }
}
