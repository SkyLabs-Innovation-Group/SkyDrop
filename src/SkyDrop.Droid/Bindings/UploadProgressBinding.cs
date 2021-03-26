using System;
using Android.Animation;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class UploadProgressBinding : MvxTargetBinding<ProgressBar, double>
    {
        public const string Name = "Progress";

        private const int increments = 10000;
        private const int animationDuration = 500;

        public UploadProgressBinding(ProgressBar target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(double value)
        {
            //value is 0-1
            Target.Max = increments;
            Target.Indeterminate = false;

            var newProgress = (int)(value * increments);
            if (value == 0)
                Target.SetProgress(newProgress, false);
            else
                SetProgressAnimate(Target, newProgress);
        }

        private void SetProgressAnimate(ProgressBar progressBar, int newProgress)
        {
            var animation = ObjectAnimator.OfInt(progressBar, "progress", progressBar.Progress, newProgress);
            animation.SetDuration(animationDuration);
            animation.SetInterpolator(new DecelerateInterpolator());
            animation.Start();
        }
    }
}
