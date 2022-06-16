using System;
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
    //cell for a grid file item in the gallery view
    public partial class FileCollectionViewCell : MvxCollectionViewCell
    {
        public static readonly NSString Key = new NSString("FileCollectionViewCell");
        public static readonly UINib Nib;

        UILongPressGestureRecognizer longPressRecognizer;

        private SkyFileDVM viewModel => DataContext as SkyFileDVM;

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
                set.Bind(SelectedIndicatorView).For(i => i.BackgroundColor).To(vm => vm.FillColor).WithConversion("NativeColor");
                set.Bind(SelectedIndicatorView).For("Visible").To(vm => vm.IsSelectionActive);
                set.Apply();
            });
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            ContainerView.BackgroundColor = Colors.DarkGrey.ToNative();
            InnerView.BackgroundColor = Colors.MidGrey.ToNative();
            BottomPanel.BackgroundColor = Colors.MidGrey.ToNative();

            longPressRecognizer = new UILongPressGestureRecognizer(LongPress);
            ContainerView.AddGestureRecognizer(longPressRecognizer);

            InnerView.Layer.CornerRadius = 8;
            InnerView.Layer.MasksToBounds = true;
        }

        public void LongPress()
        {
            if (longPressRecognizer.State == UIGestureRecognizerState.Began)
                viewModel.LongTapCommand?.Execute();

            // stop recognizing long press gesture here
            longPressRecognizer.State = UIGestureRecognizerState.Ended;
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
