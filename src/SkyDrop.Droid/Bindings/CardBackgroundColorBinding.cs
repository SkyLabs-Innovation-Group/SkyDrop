using System.Drawing;
using Acr.UserDialogs;
using AndroidX.CardView.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace SkyDrop.Droid.Bindings
{
    public class CardBackgroundColorBinding : MvxTargetBinding<CardView, Color>
    {
        public CardBackgroundColorBinding(CardView target) : base(target)
        {
        }

        public static string Name => "CardBackgroundColor";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(Color value)
        {
            Target?.SetCardBackgroundColor(value.ToNative());
        }
    }
}