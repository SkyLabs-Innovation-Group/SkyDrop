using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.Droid.Bindings
{
    public class PairingOverlayBinding : MvxTargetBinding<View, AddContactResult>
    {
        public PairingOverlayBinding(View target) : base(target)
        {
        }

        public static string Name => "PairingOverlay";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(AddContactResult value)
        {
            var checkIcon = Target.FindViewById<ImageView>(Resource.Id.checkIcon);
            switch (value)
            {
                case AddContactResult.Default:
                    Target.Visibility = ViewStates.Gone;
                    break;
                case AddContactResult.InvalidKey:
                case AddContactResult.WrongDevice:
                    Target.Visibility = ViewStates.Visible;
                    Target.SetBackgroundColor(Colors.Red.ToNative());
                    checkIcon.SetLocalImage("ic_error");
                    break;
                case AddContactResult.DevicesPaired:
                case AddContactResult.ContactAdded:
                case AddContactResult.AlreadyExists:
                    Target.Visibility = ViewStates.Visible;
                    Target.SetBackgroundColor(Colors.Primary.ToNative());
                    checkIcon.SetLocalImage("ic_check");
                    break;
            }
        }
    }
}