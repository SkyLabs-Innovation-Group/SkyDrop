using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using MvvmCross.Commands;
using UIKit;

namespace SkyDrop.iOS.Bindings
{
    /// <summary>
    /// Binds long press event to a command
    /// </summary>
    public class LongPressBinding : MvxTargetBinding<UIView, IMvxCommand>
    {
        private readonly UILongPressGestureRecognizer longPressRecognizer;
        private IMvxCommand command;

        public LongPressBinding(UIView target) : base(target)
        {
            longPressRecognizer = new UILongPressGestureRecognizer(LongPress);
            target.AddGestureRecognizer(longPressRecognizer);
        }

        public static string Name => "LongPress";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(IMvxCommand value)
        {
            command = value;
        }

        public void LongPress()
        {
            if (longPressRecognizer.State == UIGestureRecognizerState.Began)
                command?.Execute();

            // stop recognizing long press gesture here
            longPressRecognizer.State = UIGestureRecognizerState.Ended;
        }
    }
}