using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;

namespace SkyDrop.iOS.Bindings
{
    public class IconBinding : MvxTargetBinding<UIImageView, string>
    {
        public IconBinding(UIImageView target) : base(target)
        {
        }

        public static string Name => "Icon";

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