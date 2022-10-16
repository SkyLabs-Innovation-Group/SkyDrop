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

        public PortalViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
        }

        public void Bind(PortalPreferencesListAdapter adapter)
        {
            listAdapter = adapter;
            upvoteIcon = ItemView.FindViewById<ImageView>(Resource.Id.ic_upvote);
            downvoteIcon = ItemView.FindViewById<ImageView>(Resource.Id.ic_downvote);
            upvoteIcon.Click += UpvoteIcon_Click;
            downvoteIcon.Click += DownvoteIcon_Click;
        }

        private void UpvoteIcon_Click(object sender, EventArgs e)
        {

        }

        private void DownvoteIcon_Click(object sender, EventArgs e)
        {
        }
    }
}

