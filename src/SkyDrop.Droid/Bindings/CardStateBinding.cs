using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.Droid;
using System.Drawing;
using Xamarin.Essentials;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Sets material card outline color depending on bool state
    /// </summary>
    public class MaterialCardStateBinding : MvxTargetBinding<MaterialCardView, bool>
    {
        public static string Name => "MaterialCardState";

        private Color defaultColor;

        public MaterialCardStateBinding(MaterialCardView target) : base(target)
        {
            var intColor = target.StrokeColor;
            var androidColor = new Android.Graphics.Color(intColor);
            defaultColor = androidColor.ToSystemColor();
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            if (value)
            {
                Target.StrokeColor = defaultColor.ToNative();
            }
            else
            {
                Target.StrokeColor = Colors.MidGrey.ToNative();
            }
        }
    }
}