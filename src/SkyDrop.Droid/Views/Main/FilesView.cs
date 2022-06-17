using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using MvvmCross.Commands;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class FilesView : BaseActivity<FilesViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.FilesView;

        public MvxRecyclerView RecyclerView { get; set; }

        private FilesGridAdapter filesGridAdapter;
        private MvxRecyclerAdapter filesListAdapter;

        private int gridMarginPx => AndroidUtil.DpToPx(16);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            filesGridAdapter = new FilesGridAdapter(BindingContext as IMvxAndroidBindingContext) { MarginPx = gridMarginPx };
            filesListAdapter = new MvxRecyclerAdapter(BindingContext as IMvxAndroidBindingContext);

            RecyclerView = FindViewById<MvxRecyclerView>(Resource.Id.FilesRecycler);
            RecyclerView.Adapter = filesGridAdapter;

            var set = CreateBindingSet();
            set.Bind(this).For(t => t.LayoutType).To(vm => vm.LayoutType);
            set.Apply();

            Log.Trace("MainView OnCreate()");
        }

        private FileLayoutType layoutType;
        public FileLayoutType LayoutType
        {
            get => FileLayoutType.List;
            set
            {
                if (layoutType == value)
                    return;

                layoutType = value;

                if (layoutType == FileLayoutType.List)
                {
                    var layoutManager = new MvxGuardedLinearLayoutManager(this);
                    RecyclerView.SetLayoutManager(layoutManager);
                    RecyclerView.ItemTemplateId = Resource.Layout.item_file_list;
                    RecyclerView.Adapter = filesListAdapter;
                    RecyclerView.SetPadding(0, 0, 0, 0);
                }
                else
                {
                    var layoutManager = new MvxGuardedGridLayoutManager(this, 2);
                    RecyclerView.SetLayoutManager(layoutManager);
                    RecyclerView.ItemTemplateId = Resource.Layout.item_file_grid;
                    RecyclerView.Adapter = filesGridAdapter;
                    RecyclerView.SetPadding(0, 0, AndroidUtil.DpToPx(16), 0);
                }
            }
        }

        public class FilesGridAdapter : MvxRecyclerAdapter
        {
            public int MarginPx { get; set; }

            protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
            {
                //calculate view size based on screen width
                var (screenWidth, _) = AndroidUtil.GetScreenSizePx();
                var gridItemSize = (screenWidth - MarginPx) / 2;

                //make the grid items square
                var view = base.InflateViewForHolder(parent, viewType, bindingContext) as FrameLayout;
                view.LayoutParameters.Height = gridItemSize;
                view.LayoutParameters.Width = gridItemSize;
                return view;
            }

            public FilesGridAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext) { }

            public FilesGridAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        }
    }
}
