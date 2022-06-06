using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using MvvmCross.Commands;
using MvvmCross.Droid.Support.V7.RecyclerView;
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

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RecyclerView = FindViewById<MvxRecyclerView>(Resource.Id.FilesRecycler);

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
                }
                else
                {
                    var layoutManager = new MvxGuardedGridLayoutManager(this, 2);
                    RecyclerView.SetLayoutManager(layoutManager);
                    RecyclerView.ItemTemplateId = Resource.Layout.item_file_grid;
                }
            }
        }
    }
}
