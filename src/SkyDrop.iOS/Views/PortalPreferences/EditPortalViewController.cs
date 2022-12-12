using SkyDrop.Core.ViewModels;

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

            PortalApiTokenTextView.Selectable = false;
            var set = CreateBindingSet();
            set.Bind(PortalNameTextView).For(v => v.Text).To(vm => vm.PortalName);
            set.Bind(PortalUrlTextView).For(v => v.Text).To(vm => vm.PortalUrl);
            set.Bind(PortalApiTokenTextView).For(v => v.Text).To(vm => vm.ApiToken);
            set.Bind(PortalApiTokenTextView).For("Tap").To(vm => vm.LoginWithPortalCommand);
            set.Bind(SaveButton).For("Tap").To(vm => vm.SavePortalCommand);
            set.Apply();
        }
    }
}