using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.iOS.Bindings
{
    public class FileCategoryIconBinding : MvxTargetBinding<UIImageView, string>
    {
        public FileCategoryIconBinding(UIImageView target) : base(target)
        {
        }

        public static string Name => "FileCategoryIcon";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string value)
        {
            var imageRes = GetImageResource(value);
            Target.Image = UIImage.FromBundle(imageRes);
        }

        private string GetImageResource(string filename)
        {
            var fileCategory = filename.GetFileCategory();
            switch (fileCategory)
            {
                case FileCategory.Document:
                    return "ic_file_document";
                case FileCategory.Image:
                    return "ic_file_image";
                case FileCategory.Audio:
                    return "ic_file_audio";
                case FileCategory.Video:
                    return "ic_file_video";
                case FileCategory.Encrypted:
                    return "ic_file_encrypted";
                case FileCategory.Zip:
                    return "ic_zip";
                case FileCategory.None:
                default:
                    return "ic_file_generic";
            }
        }
    }
}