using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using static Android.Views.View;

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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var portalViewHolder = (PortalViewHolder) holder;
            portalViewHolder.Bind();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_skynet_portal, parent, false);
            return new PortalViewHolder(itemView, BindingContext);
        }
    }
}

