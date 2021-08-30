using System;
using Android.Animation;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class UploadProgressBinding : MvxTargetBinding<ProgressBar, int>
    {
        public const string Name = "Progress";

        private const int animationDuration = 500;

        public UploadProgressBinding(ProgressBar target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(int value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value));

            //value is 0-1
            Target.Max = 100;
            Target.Indeterminate = false;

            var newProgress = value;
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
