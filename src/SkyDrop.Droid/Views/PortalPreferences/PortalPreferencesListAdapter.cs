using System;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    public class PortalPreferencesListAdapter : MvxRecyclerAdapter
    {
        private readonly PortalPreferencesListView view;

        public PortalPreferencesListAdapter(PortalPreferencesListView view, IMvxAndroidBindingContext bindingContext) : base(bindingContext)
        {
            this.view = view;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var portalViewHolder = (PortalViewHolder) holder;
            portalViewHolder.Bind(this);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_skynet_portal, parent, false);
            return new PortalViewHolder(itemView, BindingContext);
        }

        internal void ReorderPortals(int position, int newPosition) => view.ReorderPortals(position, newPosition);
    }
}

