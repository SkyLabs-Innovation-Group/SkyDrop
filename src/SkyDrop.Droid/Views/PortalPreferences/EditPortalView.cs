using System;
using Android.App;
using System.Reflection.Emit;
using SkyDrop.Core.ViewModels;
using Android.OS;
using Android.Widget;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    [Activity(Label = "EditPortalView")]
    public class EditPortalView : BaseActivity<EditPortalViewModel>
    {
        public EditPortalView()
        {
        }

        protected override int ActivityLayoutId => Resource.Layout.EditPortalView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var portalApiTextView = FindViewById<EditText>(Resource.Id.portalApiTokenEditText)!;
            portalApiTextView.Focusable = false;
        }
    }
}

