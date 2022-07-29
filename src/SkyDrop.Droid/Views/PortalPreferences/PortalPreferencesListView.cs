using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    public class PortalPreferencesListView : MvxRecyclerView
    {
        private PortalPreferencesListAdapter portalPrefsAdapter;

        public PortalPreferencesListView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected PortalPreferencesListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }

        public void Init(IMvxBindingContext bindingContext)
        {
            portalPrefsAdapter = new PortalPreferencesListAdapter(bindingContext as IMvxAndroidBindingContext);
            Adapter = portalPrefsAdapter;
        }

        
    }

    internal class PortalPreferencesListAdapter : MvxRecyclerAdapter
    {
        public PortalPreferencesListAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext) { }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            //make the grid items square
            var view = base.InflateViewForHolder(parent, viewType, bindingContext) as LinearLayout;

            

            return view;
        }

    }
}

