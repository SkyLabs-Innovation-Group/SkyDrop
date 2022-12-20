using Android.Support.V7.Widget;
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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            return new MvxRecyclerViewHolder(InflateViewForHolder(parent, viewType, itemBindingContext),
                itemBindingContext);
        }
    }
}