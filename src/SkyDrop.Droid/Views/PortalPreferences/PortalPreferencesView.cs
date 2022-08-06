
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using SkyDrop.Core.DataModels;
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

        internal void ReorderPortals(int position, int newPosition) => ViewModel?.ReorderPortals(position, newPosition);

        private void BindViews()
        {
            portalPrefsListview = FindViewById<PortalPreferencesListView>(Resource.Id.PortalPreferencesListView);
            portalPrefsListview.Init(this, BindingContext as IMvxAndroidBindingContext);
            //saveButton = FindViewById<Button>(Resource.Id.saveButton);
        }
    }
}

