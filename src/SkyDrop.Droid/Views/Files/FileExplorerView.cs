using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Files
{
	public class FileExplorerView : MvxRecyclerView
	{
        private FilesGridAdapter filesGridAdapter;
        private MvxRecyclerAdapter filesListAdapter;
        private int gridMarginPx => AndroidUtil.DpToPx(16);

        public FileExplorerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {

        }

        public FileExplorerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {

        }

        public void Init(IMvxBindingContext bindingContext)
        {
            filesGridAdapter = new FilesGridAdapter(bindingContext as IMvxAndroidBindingContext) { MarginPx = gridMarginPx };
            filesListAdapter = new MvxRecyclerAdapter(bindingContext as IMvxAndroidBindingContext);
            Adapter = filesGridAdapter;
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
                    var layoutManager = new MvxGuardedLinearLayoutManager(Context);
                    SetLayoutManager(layoutManager);
                    ItemTemplateId = Resource.Layout.item_file_list;
                    Adapter = filesListAdapter;
                    SetPadding(0, 0, 0, 0);
                }
                else
                {
                    var layoutManager = new MvxGuardedGridLayoutManager(Context, 2);
                    SetLayoutManager(layoutManager);
                    ItemTemplateId = Resource.Layout.item_file_grid;
                    Adapter = filesGridAdapter;
                    SetPadding(0, 0, AndroidUtil.DpToPx(16), 0);
                }
            }
        }

        private class FilesGridAdapter : MvxRecyclerAdapter
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

