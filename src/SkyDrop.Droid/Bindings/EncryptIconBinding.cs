using System.Drawing;
using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    public class EncryptIconBinding : MvxTargetBinding<ImageView, string>
    {
        public static string Name => "EncryptIcon";

        public EncryptIconBinding(ImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string value)
        {
            var isAnyoneWithTheLink = value == new AnyoneWithTheLinkItem().Name;
            var icon = isAnyoneWithTheLink ? "ic_world" : "ic_key";
            Target.SetLocalImage(icon);
        }
    }
}