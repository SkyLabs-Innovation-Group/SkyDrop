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
        
        private IMenu menu;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            UploadedFilesRecyclerView = FindViewById<RecyclerView>(Resource.Id.UploadedFilesList);

            await ViewModel.InitializeTask.Task;
            ViewModel.PropertyChanged += HandlePropertyChanged;

            Log.Trace("MainView OnCreate()");

            ViewModel.SelectFileAsyncFunc = () => AndroidUtil.SelectFile(this);
            ViewModel.SelectImageAsyncFunc = () => AndroidUtil.SelectImage(this);
            ViewModel.FileTapCommand = new MvxCommand<SkyFile>(skyFile => AndroidUtil.OpenFileInBrowser(this, skyFile));
            ViewModel.AfterFileSelected = new MvxCommand(() => AfterFileWasSelected());
            ViewModel.ScrollToFileCommand = new MvxCommand(() => ScrollToFile());
        }

        private void HandlePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ViewModel.SkyFiles))
            {
                var uploadItem = menu.FindItem(Resource.Id.menu_files_upload);
                if (ViewModel.SkyFiles.Where(sf => sf.SkyFile.Status == FileStatus.Staged).Count() > 0)
                    uploadItem.SetIcon(GetDrawable(Resource.Drawable.ic_upload));
                else
                    uploadItem.SetIcon(GetDrawable(Resource.Drawable.ic_upload_grey));
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.FilesMenu, menu);
            this.menu = menu;
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

        private void ScrollToFile()
        {
            UploadedFilesRecyclerView.SmoothScrollToPosition(0);
        }

        private void AfterFileWasSelected()
        {
            int? previouslySelectedIndex = ViewModel.GetIndexForPreviouslySelectedFile();

            if (previouslySelectedIndex == null)
            {
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(ViewModel.CurrentlySelectedFileIndex);
            }
            else if (ViewModel.CurrentlySelectedFileIndex == previouslySelectedIndex.Value)
            {
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(ViewModel.CurrentlySelectedFileIndex);
            }
            else
            {
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(previouslySelectedIndex.Value);
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(ViewModel.CurrentlySelectedFileIndex);
            }
        }

        protected override async void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            try
            {
                if (requestCode == AndroidUtil.PickFileRequestCode)
                {
                    if (data == null)
                        return;

                    await HandlePickedFile(data);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async Task HandlePickedFile(Intent data)
        {
            var stagedFile = await AndroidUtil.HandlePickedFile(this, data);
            ViewModel.StageFile(stagedFile);
        }
    }
}
