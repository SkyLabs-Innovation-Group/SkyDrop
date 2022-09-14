using System;
using Android.Content.Res;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binding for use with SaveUnzipIconConverter
    /// Expects icons in this format "res:ic_download"
    /// </summary>
    public class IconBinding : MvxTargetBinding<ImageView, string>
    {
        public static string Name => "Icon";

        public IconBinding(ImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string value)
        {
            var identifier = value.Substring(4); //remove "res:" prefix
            Target.SetLocalImage(identifier);
        }
    }
}
