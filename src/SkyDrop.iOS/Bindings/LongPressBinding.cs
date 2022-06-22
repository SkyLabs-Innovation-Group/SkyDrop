using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using System.Threading.Tasks;
using System;
using System.IO;
using FFImageLoading;
using MvvmCross;
using SkyDrop;
using UIKit;
using Foundation;
using Serilog;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;
using MvvmCross.Commands;

namespace SkyDrop.iOS.Bindings
{
    /// <summary>
    /// Binds long press event to a command
    /// </summary>
    public class LongPressBinding : MvxTargetBinding<UIView, IMvxCommand>
    {
        public static string Name => "LongPress";

        private UILongPressGestureRecognizer longPressRecognizer;
        private IMvxCommand command;

        public LongPressBinding(UIView target) : base(target)
        {
            longPressRecognizer = new UILongPressGestureRecognizer(LongPress);
            target.AddGestureRecognizer(longPressRecognizer);
        }

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