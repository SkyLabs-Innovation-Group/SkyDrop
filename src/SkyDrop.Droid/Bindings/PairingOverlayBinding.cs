using System.Drawing;
using Acr.UserDialogs;
using Android.Views;
using AndroidX.CardView.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.Droid.Bindings
{
    public class PairingOverlayBinding : MvxTargetBinding<View, AddContactResult>
    {
        public static string Name => "PairingOverlay";

        public PairingOverlayBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(AddContactResult value)
        {
            switch(value)
            {
                case AddContactResult.Default:
                case AddContactResult.InvalidKey:
                case AddContactResult.WrongDevice:
                    Target.Visibility = ViewStates.Gone;
                    break;
                case AddContactResult.DevicesPaired:
                case AddContactResult.ContactAdded:
                case AddContactResult.AlreadyExists:
                    Target.Visibility = ViewStates.Visible;
                    break;
            }
        }
    }
}