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
using SkyDrop.Droid.Views.Files;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class FilesView : BaseActivity<FilesViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.FilesView;

        public FileExplorerView FileExplorerView { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            FileExplorerView = FindViewById<FileExplorerView>(Resource.Id.FilesRecycler);
            FileExplorerView.Init(BindingContext);

            var set = CreateBindingSet();
            set.Bind(FileExplorerView).For(t => t.LayoutType).To(vm => vm.LayoutType);
            set.Apply();
        }

        public override void OnBackPressed()
        {
            ViewModel.BackCommand?.Execute();
        }
    }
}
