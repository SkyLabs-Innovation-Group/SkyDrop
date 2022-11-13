using System;
using System.Linq;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.iOS.Styles;
using UIKit;

namespace SkyDrop.iOS.Bindings
{
    public class NextButtonStyleBinding : MvxTargetBinding<UIView, bool>
    {
        public static string Name => "NextButtonStyle";

        public NextButtonStyleBinding(UIView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool isDoneButton)
        {
            var label = Target.Subviews.FirstOrDefault(v => v is UILabel) as UILabel;
            var icon = Target.Subviews.FirstOrDefault(v => v is UIImageView) as UIImageView;
            if (isDoneButton)
            {
                Target.StyleButton(Colors.Primary);
                icon.Image = UIImage.FromBundle("ic_tick");
                //icon.ContentMode = UIViewContentMode.ScaleAspectFit;
                label.Text = "Done";
            }
            else
            {
                Target.StyleButton(Colors.Primary, true);
                icon.Image = UIImage.FromBundle("ic_next");
                //icon.ContentMode = UIViewContentMode.Center;
                label.Text = "Next";
            }
        }
    }
}
