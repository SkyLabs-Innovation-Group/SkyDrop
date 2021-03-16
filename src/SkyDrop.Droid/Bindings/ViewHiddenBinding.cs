using System;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class ViewHiddenBinding : MvxTargetBinding<View, bool>
    {
        public static string Name => "Hidden";

        public ViewHiddenBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            Target.Visibility = value ? ViewStates.Invisible : ViewStates.Visible;
        }
    }
}
