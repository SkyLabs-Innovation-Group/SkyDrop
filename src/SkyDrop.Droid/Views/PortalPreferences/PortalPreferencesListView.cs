using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using static Android.Views.View;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    public class PortalPreferencesListView : MvxRecyclerView
    {
        private PortalPreferencesView view;
        private PortalPreferencesListAdapter portalPrefsAdapter;

        public PortalPreferencesListView(Android.Content.Context context, IAttributeSet attrs) : base(context, attrs) { }

        protected PortalPreferencesListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }

        public void Init(PortalPreferencesView view, IMvxAndroidBindingContext bindingContext)
        {
            portalPrefsAdapter = new PortalPreferencesListAdapter(this, bindingContext);
            Adapter = portalPrefsAdapter;
            this.view = view;
        }

        internal void ReorderPortals(int position, int newPosition) => view.ReorderPortals(position, newPosition);
    }
}

