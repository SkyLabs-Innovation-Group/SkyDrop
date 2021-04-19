using System.Drawing;
using Acr.UserDialogs;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using UIKit;

namespace Engage.iOS.Bindings
{
    public class ProgressFillHeightBinding : MvxTargetBinding<UIView, double>
    {
        public static string Name => "ProgressFillHeight";

        private NSLayoutConstraint heightConstraint;

        public ProgressFillHeightBinding(UIView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(double value)
        {
            var parentHeight = Target.Superview.Frame.Height;
            var progressHeight = parentHeight * value;

            if (heightConstraint == null)
            {
                heightConstraint = Target.HeightAnchor.ConstraintEqualTo((float)progressHeight);
                heightConstraint.Active = true;
            }
            else
                heightConstraint.Constant = (float)progressHeight;
        }
    }
}