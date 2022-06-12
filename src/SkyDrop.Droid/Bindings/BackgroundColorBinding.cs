using System.Drawing;
using Acr.UserDialogs;
using Android.Views;
using AndroidX.CardView.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class BackgroundColorBinding : MvxTargetBinding<View, Color>
    {
        public static string Name => "BackgroundColor";

        public BackgroundColorBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(Color value)
        {
            Target?.SetBackgroundColor(value.ToNative());
        }
    }
}