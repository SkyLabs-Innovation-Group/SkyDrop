using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using MvvmCross.Commands;

namespace SkyDrop.Droid.Bindings
{
    public class ButtonClickBinding : MvxTargetBinding<Button, IMvxCommand>
    {
        public ButtonClickBinding(Button target) : base(target)
        {
        }

        public static string Name => "Click";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(IMvxCommand value)
        {
            Target.Click += (s, e) => value.Execute();
        }
    }
}