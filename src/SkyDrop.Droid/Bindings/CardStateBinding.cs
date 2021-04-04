using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using Engage.Droid;
using System.Drawing;
using Xamarin.Essentials;

namespace Engage.Droid.Bindings
{
    /// <summary>
    /// Sets material card fill to light grey or dark grey with mid grey outline depending on bool state
    /// </summary>
    public class MaterialCardStateBinding : MvxTargetBinding<MaterialCardView, bool>
    {
        public static string Name => "MaterialCardState";

        private Color defaultColor;

        public MaterialCardStateBinding(MaterialCardView target) : base(target)
        {
            var intColor = target.CardBackgroundColor.DefaultColor;
            var androidColor = new Android.Graphics.Color(intColor);
            defaultColor = androidColor.ToSystemColor();
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            if (value)
            {
                Target.SetCardBackgroundColor(defaultColor.ToNative());
                Target.StrokeColor = defaultColor.ToNative();
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
                var label = parent.FindViewById<TextView>(SkyDrop.Droid.Resource.Id.ButtonLabel);
                var icon = parent.FindViewById<ImageView>(SkyDrop.Droid.Resource.Id.ButtonIcon);
                label?.SetTextColor(childColor);
                icon?.SetColorFilter(childColor);
            }
        }
    }
}