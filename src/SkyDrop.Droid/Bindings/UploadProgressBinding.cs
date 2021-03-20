using System;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class UploadProgressBinding : MvxTargetBinding<ProgressBar, double>
    {
        public static string Name => "Progress";

        public UploadProgressBinding(ProgressBar target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(double value)
        {
            //value is 0-1
            var max = 100;
            Target.Max = max;
            Target.Indeterminate = false;
            Target.SetProgress((int)(value * max), true);
        }
    }
}
