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

        public RecyclerView UploadedFilesRecyclerView { get; set; }

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            UploadedFilesRecyclerView = FindViewById<RecyclerView>(Resource.Id.UploadedFilesList);

            await ViewModel.InitializeTask.Task;

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
    }
}
