using System.Drawing;
using Acr.UserDialogs;
using AndroidX.CardView.Widget;
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
            var backgroundColor = value ? Colors.DarkGrey : Colors.White;
            var strokeColor = value ? Colors.MidGrey : Colors.White;
            Target.SetCardBackgroundColor(backgroundColor.ToNative());
            Target.StrokeColor = strokeColor.ToNative();
            Target.StrokeWidth = AndroidUtil.DpToPx(1);
        }
    }
}