
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Styles;

namespace SkyDrop.Droid.Bindings
{
    public class NextButtonStyleBinding : MvxTargetBinding<MaterialCardView, bool>
    {
        public static string Name => "NextButtonStyle";

        public NextButtonStyleBinding(MaterialCardView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool isDoneButton)
        {
            var label = Target.FindViewByType<TextView>();
            var icon = Target.FindViewByType<AppCompatImageView>();
            if (isDoneButton)
            {
                Target.StyleButton(Colors.Primary);
                icon.SetLocalImage("ic_check");
                icon.RotationY = 0;
                label.Text = "Done";
            }
            else
            {
                Target.StyleButton(Colors.Primary, true);
                icon.SetLocalImage("ic_back");
                icon.RotationY = 180;
                label.Text = "Next";
            }
        }
    }
}
