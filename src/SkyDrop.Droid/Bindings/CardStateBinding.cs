using System.Drawing;
using Acr.UserDialogs;
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
                Target.SetCardBackgroundColor(Colors.LightGrey.ToNative());
                Target.StrokeColor = Colors.LightGrey.ToNative();
            }
            else
            {
                Target.SetCardBackgroundColor(Colors.DarkGrey.ToNative());
                Target.StrokeColor = Colors.MidGrey.ToNative();
            }
        }
    }
}