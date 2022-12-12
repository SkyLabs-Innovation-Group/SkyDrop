using System.Drawing;
using Acr.UserDialogs;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class BackgroundColorBinding : MvxTargetBinding<View, Color>
    {
        public BackgroundColorBinding(View target) : base(target)
        {
        }

        public static string Name => "BackgroundColor";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(Color value)
        {
            Target?.SetBackgroundColor(value.ToNative());
        }
    }
}