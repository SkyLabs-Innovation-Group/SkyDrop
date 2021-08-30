using System;
using System.Drawing;
using Acr.UserDialogs;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Bindings
{
    public class ProgressFillHeightBinding : MvxTargetBinding<UIView, int>
    {
        public static string Name => "ProgressFillHeight";

        private NSLayoutConstraint heightConstraint;

        public ProgressFillHeightBinding(UIView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(int value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value));

            var parentHeight = Target.Superview.Frame.Height;
            var progressHeight = parentHeight * Math.Min(value, 100) / 100;

            if (heightConstraint == null)
            {
                heightConstraint = Target.HeightAnchor.ConstraintEqualTo(0);
                heightConstraint.Active = true;
            }

            Target.Superview.LayoutIfNeeded();

            var duration = value >= 1 || value == 0 ? 0.1 : 1.5;

            UIView.Animate(duration, () =>
            {
                heightConstraint.Constant = (float)progressHeight;

                Target.Superview.LayoutIfNeeded();
            });
        }
    }
}