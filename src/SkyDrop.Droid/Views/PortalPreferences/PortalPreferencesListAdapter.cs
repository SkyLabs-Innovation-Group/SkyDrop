﻿using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    public class PortalPreferencesListAdapter : MvxRecyclerAdapter
    {
        public readonly PortalPreferencesListView View;

        public PortalPreferencesListAdapter(PortalPreferencesListView view, IMvxAndroidBindingContext bindingContext) :
            base(bindingContext)
        {
            View = view;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            var portalViewHolder = holder as PortalViewHolder;
            portalViewHolder.Bind(this, position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            return new PortalViewHolder(InflateViewForHolder(parent, viewType, itemBindingContext), itemBindingContext);
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