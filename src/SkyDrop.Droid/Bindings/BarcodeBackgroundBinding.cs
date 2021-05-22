using Acr.UserDialogs;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    public class BarcodeBackgroundBinding : MvxTargetBinding<MaterialCardView, bool>
    {
        public static string Name => "BarcodeBackground";

        public BarcodeBackgroundBinding(MaterialCardView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            var backgroundColor = value ? Colors.DarkGrey : Colors.LightGrey;
            var strokeColor = value ? Colors.MidGrey : Colors.LightGrey;
            Target.SetCardBackgroundColor(backgroundColor.ToNative());
            Target.StrokeColor = strokeColor.ToNative();
            Target.StrokeWidth = AndroidUtil.DpToPx(1);
        }
    }
}