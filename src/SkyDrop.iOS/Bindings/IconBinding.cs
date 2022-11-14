using System.Drawing;
using Acr.UserDialogs;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using UIKit;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.iOS.Bindings
{
    public class IconBinding : MvxTargetBinding<UIImageView, string>
    {
        public static string Name => "Icon";

        public IconBinding(UIImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string imageRes)
        {
            try
            {
                Target.Image = UIImage.FromBundle(imageRes);
            }
            catch
            {
            }
        }
    }
}