using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    public class PortalPreferencesListView : MvxRecyclerView
    {
        private PortalPreferencesListAdapter portalPrefsAdapter;
        public PortalPreferencesView View;

        public PortalPreferencesListView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected PortalPreferencesListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference,
            transfer)
        {
        }

        public void Init(PortalPreferencesView view, IMvxAndroidBindingContext bindingContext)
        {
            portalPrefsAdapter = new PortalPreferencesListAdapter(this, bindingContext);
            Adapter = portalPrefsAdapter;
            View = view;
        }

        internal void EditPortal(int position)
        {
            View.EditPortal(position);
        }

        internal void ReorderPortals(int position, int newPosition)
        {
            View.ReorderPortals(position, newPosition);
        }
    }
}