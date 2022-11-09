using System;
using Android.App;
using System.Reflection.Emit;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    [Activity(Label = "EditPortalView")]
    public class EditPortalView : BaseActivity<PortalPreferencesViewModel>
    {
        public EditPortalView()
        {
        }

        protected override int ActivityLayoutId => Resource.Layout.EditPortalView;
    }
}

