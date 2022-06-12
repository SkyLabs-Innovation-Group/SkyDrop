using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            filesGridAdapter = new FilesGridAdapter(BindingContext as IMvxAndroidBindingContext);
            filesListAdapter = new MvxRecyclerAdapter(BindingContext as IMvxAndroidBindingContext);

            RecyclerView = FindViewById<MvxRecyclerView>(Resource.Id.FilesRecycler);
            RecyclerView.Adapter = filesGridAdapter;

            var set = CreateBindingSet();
            set.Bind(this).For(t => t.LayoutType).To(vm => vm.LayoutType);
            set.Apply();

            Log.Trace("MainView OnCreate()");
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.FilesMenu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_files_upload:
                    ViewModel.UploadCommand?.Execute();
                    break;
            }

            return true;
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
                }
                else
                {
                    var layoutManager = new MvxGuardedGridLayoutManager(this, 2);
                    RecyclerView.SetLayoutManager(layoutManager);
                    RecyclerView.ItemTemplateId = Resource.Layout.item_file_grid;
                    RecyclerView.Adapter = filesGridAdapter;
                }
            }
        }

        public class FilesGridAdapter : MvxRecyclerAdapter
        {
            protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
            {
                //calculate view size based on screen width
                var (screenWidth, _) = AndroidUtil.GetScreenSizePx();
                var gridItemSize = screenWidth / 2;
                int previewCardSize = (int)(gridItemSize * 0.7);

                //make the grid items square
                var view = base.InflateViewForHolder(parent, viewType, bindingContext) as LinearLayout;
                view.LayoutParameters.Height = gridItemSize;
                var previewCard = view.FindViewById<CardView>(Resource.Id.PreviewCard);
                previewCard.LayoutParameters.Width = previewCardSize;
                previewCard.LayoutParameters.Height = previewCardSize;
                return view;
            }

            public FilesGridAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext) { }

            public FilesGridAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        }
    }
}
