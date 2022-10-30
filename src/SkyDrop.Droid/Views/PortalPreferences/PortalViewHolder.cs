using System;
using System.Diagnostics;
using Android.Views;
using Android.Widget;
using Java.Interop;
using MvvmCross;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.PortalPreferences
{
  public class PortalViewHolder : MvxRecyclerViewHolder
  {
        private PortalPreferencesListAdapter listAdapter;
        private ImageView upvoteIcon;
        private ImageView downvoteIcon;
        private ImageView reorderIcon;
        private int position;

        public PortalViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
            upvoteIcon = ItemView.FindViewById<ImageView>(Resource.Id.upvoteButton);
            downvoteIcon = ItemView.FindViewById<ImageView>(Resource.Id.downvoteButton);
            upvoteIcon.Click += UpvoteIcon_Click;
            downvoteIcon.Click += DownvoteIcon_Click;
        }

        public void Bind(PortalPreferencesListAdapter adapter, int position)
        {
            listAdapter = adapter;
            this.position = position;
        }

        private void UpvoteIcon_Click(object sender, EventArgs e)
        {
            var vm = listAdapter.View.View.ViewModel;
            vm.ReorderPortals(position, position - 1);
        }

        private void DownvoteIcon_Click(object sender, EventArgs e)
        {
            var vm = listAdapter.View.View.ViewModel;
            vm.ReorderPortals(position, position + 1);
        }
    }
}

