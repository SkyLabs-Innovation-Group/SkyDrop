using System;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class VisibleHiddenBinding : MvxTargetBinding<View, bool>
    {
        public static string Name => "VisibleHidden";

        public VisibleHiddenBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            Target.Visibility = value ? ViewStates.Visible : ViewStates.Invisible;
        }
    }
}
