using System;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    /*
    public class PortalViewHolder : MvxRecyclerViewHolder
    {
        private readonly ImageView downvoteIcon;
        private PortalPreferencesListAdapter listAdapter;
        private int position;
        private ImageView reorderIcon;
        private readonly ImageView upvoteIcon;

        public PortalViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
            upvoteIcon = ItemView.FindViewById<ImageView>(Resource.Id.upvoteButton);
            downvoteIcon = ItemView.FindViewById<ImageView>(Resource.Id.downvoteButton);
            upvoteIcon.Click += UpvoteIcon_Click;
            downvoteIcon.Click += DownvoteIcon_Click;
            itemView.Click += ItemView_Click;
        }

        private void ItemView_Click(object sender, EventArgs e)
        {
            listAdapter.EditPortal(position);
        }

        public void Bind(PortalPreferencesListAdapter adapter, int position)
        {
            listAdapter = adapter;
            this.position = position;
        }
    }
    */
}