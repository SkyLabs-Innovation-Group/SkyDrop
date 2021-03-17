using System.Drawing;
using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Plugin.CurrentActivity;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;

namespace Engage.Droid.Bindings
{
    /// <summary>
    /// Sets material card fill to light grey or dark grey with mid grey outline depending on bool state
    /// </summary>
    public class MaterialCardStateBinding : MvxTargetBinding<MaterialCardView, bool>
    {
        public static string Name => "MaterialCardState";

        public MaterialCardStateBinding(MaterialCardView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            if (value)
            {
                Target.SetCardBackgroundColor(Colors.PrimaryDark.ToNative());
                Target.StrokeColor = Colors.PrimaryDark.ToNative();
            }
            else
            {
                Target.SetCardBackgroundColor(Colors.DarkGrey.ToNative());
                Target.StrokeColor = Colors.MidGrey.ToNative();
            }

            SetChildColors(Target, value);
        }

        private void SetChildColors(ViewGroup parent, bool state)
        {
            var childColor = state ? Colors.LightGrey.ToNative() : Colors.MidGrey.ToNative();
            for (var i = 0; i < parent.ChildCount; i++)
            {
                var label = parent.FindViewById<TextView>(Resource.Id.ButtonLabel);
                var icon = parent.FindViewById<ImageView>(Resource.Id.ButtonIcon);
                label?.SetTextColor(childColor);
                icon?.SetColorFilter(childColor);
            }
        }
    }
}