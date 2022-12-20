using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binding for use with SaveUnzipIconConverter
    /// Expects icons in these formats: "res:ic_download", "ic_download"
    /// </summary>
    public class IconBinding : MvxTargetBinding<ImageView, string>
    {
        public IconBinding(ImageView target) : base(target)
        {
        }

        public static string Name => "Icon";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string value)
        {
            if (value.StartsWith("res:"))
                value = value?.Substring(4); //remove "res:" prefix
            Target.SetLocalImage(value);
        }
    }
}