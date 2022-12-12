using Android.App;
using Android.OS;
using Android.Widget;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    [Activity(Label = "EditPortalView")]
    public class EditPortalView : BaseActivity<EditPortalViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.EditPortalView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var portalApiTextView = FindViewById<EditText>(Resource.Id.portalApiTokenEditText)!;
            portalApiTextView.Focusable = false;
        }
    }
}