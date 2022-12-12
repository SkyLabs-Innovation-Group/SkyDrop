using Android.App;
using Android.OS;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    [Activity(Label = "PortalPreferencesView")]
    public class PortalPreferencesView : BaseActivity<PortalPreferencesViewModel>
    {
        private PortalPreferencesListView portalPrefsListview;

        protected override int ActivityLayoutId => Resource.Layout.PortalPreferencesView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BindViews();
        }

        internal void EditPortal(int position)
        {
            ViewModel.EditPortal(position);
        }

        internal void ReorderPortals(int position, int newPosition)
        {
            ViewModel?.ReorderPortals(position, newPosition);
        }

        private void BindViews()
        {
            portalPrefsListview = FindViewById<PortalPreferencesListView>(Resource.Id.PortalPreferencesListView);
            portalPrefsListview.Init(this, BindingContext as IMvxAndroidBindingContext);
            //saveButton = FindViewById<Button>(Resource.Id.saveButton);
        }
    }
}