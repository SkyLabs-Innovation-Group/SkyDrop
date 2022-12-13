using Acr.UserDialogs;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.PortalPreferences
{
    internal partial class EditPortalViewController : BaseViewController<EditPortalViewModel>
    {
        public EditPortalViewController() : base(nameof(EditPortalViewController), null)
        {
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            PortalNameInputContainer.BackgroundColor = Colors.MidGrey.ToNative();
            PortalUrlInputContainer.BackgroundColor = Colors.MidGrey.ToNative();
            PortalApiTokenInputContainer.BackgroundColor = Colors.MidGrey.ToNative();
            PortalNameInputContainer.Layer.CornerRadius = 8;
            PortalUrlInputContainer.Layer.CornerRadius = 8;
            PortalApiTokenInputContainer.Layer.CornerRadius = 8;

            PortalNameInput.BorderStyle = UITextBorderStyle.None;
            PortalUrlInput.BorderStyle = UITextBorderStyle.None;
            PortalApiTokenInput.BorderStyle = UITextBorderStyle.None;
            PortalNameInput.TextColor = Colors.White.ToNative();
            PortalUrlInput.TextColor = Colors.White.ToNative();
            PortalApiTokenInput.TextColor = Colors.White.ToNative();

            SaveButton.BackgroundColor = Colors.Primary.ToNative();
            SaveButton.Layer.CornerRadius = 8;

            PasteApiKeyButton.BackgroundColor = Colors.MidGrey.ToNative();
            PasteApiKeyButton.Layer.CornerRadius = 8;
            PasteIcon.TintColor = Colors.LightGrey.ToNative();

            var set = CreateBindingSet();
            set.Bind(PortalNameInput).For(v => v.Text).To(vm => vm.PortalName);
            set.Bind(PortalUrlInput).For(v => v.Text).To(vm => vm.PortalUrl);
            set.Bind(PortalApiTokenInput).For(v => v.Text).To(vm => vm.ApiToken);
            set.Bind(PortalApiTokenInput).For("Tap").To(vm => vm.LoginWithPortalCommand);
            set.Bind(PortalApiTokenInputContainer).For("Tap").To(vm => vm.LoginWithPortalCommand);
            set.Bind(SaveButton).For("Tap").To(vm => vm.SavePortalCommand);
            set.Bind(PasteApiKeyButton).For("Tap").To(vm => vm.PasteApiKeyCommand);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();
        }
    }
}