using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Droid.Bindings
{
    public class FileCategoryIconBinding : MvxTargetBinding<ImageView, string>
    {
        public FileCategoryIconBinding(ImageView target) : base(target)
        {
        }

        public static string Name => "FileCategoryIcon";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string value)
        {
            var imageRes = GetImageResource(value);
            Target?.SetImageResource(imageRes);
        }

        private int GetImageResource(string filename)
        {
            var fileCategory = GetFileCategory(filename);
            switch (fileCategory)
            {
                case FileCategory.Document:
                    return Resource.Drawable.ic_file_document;
                case FileCategory.Image:
                    return Resource.Drawable.ic_file_image;
                case FileCategory.Audio:
                    return Resource.Drawable.ic_file_audio;
                case FileCategory.Video:
                    return Resource.Drawable.ic_file_video;
                case FileCategory.Encrypted:
                    return Resource.Drawable.ic_file_encrypted;
                case FileCategory.Zip:
                    return Resource.Drawable.ic_zip;
                case FileCategory.None:
                default:
                    return Resource.Drawable.ic_file_generic;
            }
        }
    }
}