using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Views.Files;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, Exported = true)]
    public class FilesView : BaseActivity<FilesViewModel>
    {
        private ImageView buttonAddFolder;
        private ImageView buttonDeleteFile;
        private ImageView buttonMoveFile;
        private ImageView buttonSaveFile;

        private ImageView buttonSelectAll;
        private ImageView buttonToggleLayout;
        protected override int ActivityLayoutId => Resource.Layout.FilesView;

        public FileExplorerView FileExplorerView { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ViewModel.CloseKeyboardCommand = new MvxAsyncCommand(this.HideKeyboard);
            ViewModel.UpdateTopBarButtonsCommand = new MvxCommand(UpdateTopBarButtons);

            FileExplorerView = FindViewById<FileExplorerView>(Resource.Id.FilesRecycler);
            FileExplorerView.Init(BindingContext);

            buttonSelectAll = FindViewById<ImageView>(Resource.Id.ButtonSelectAll);
            buttonToggleLayout = FindViewById<ImageView>(Resource.Id.ButtonToggleLayout);
            buttonAddFolder = FindViewById<ImageView>(Resource.Id.ButtonAddFolder);
            buttonDeleteFile = FindViewById<ImageView>(Resource.Id.ButtonDeleteFile);
            buttonMoveFile = FindViewById<ImageView>(Resource.Id.ButtonMoveFile);
            buttonSaveFile = FindViewById<ImageView>(Resource.Id.ButtonSaveFile);

            var set = CreateBindingSet();
            set.Bind(FileExplorerView).For(t => t.LayoutType).To(vm => vm.LayoutType);
            set.Apply();
        }

        public override void OnBackPressed()
        {
            ViewModel.BackCommand?.Execute();
        }

        private void UpdateTopBarButtons()
        {
            buttonSelectAll.Visibility = ViewModel.IsSelectAllButtonVisible.ToVisibility();
            buttonToggleLayout.Visibility = ViewModel.IsLayoutButtonVisible.ToVisibility();
            buttonAddFolder.Visibility = ViewModel.IsAddFolderButtonVisible.ToVisibility();
            buttonDeleteFile.Visibility = ViewModel.IsDeleteButtonVisible.ToVisibility();
            buttonMoveFile.Visibility = ViewModel.IsMoveButtonVisible.ToVisibility();
            buttonSaveFile.Visibility = ViewModel.IsSaveButtonVisible.ToVisibility();
        }
    }
}