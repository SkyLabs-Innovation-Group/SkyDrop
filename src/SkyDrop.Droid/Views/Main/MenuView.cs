using Android.App;
using Android.OS;
using Android.Views;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class MenuView : BaseActivity<MenuViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.MenuView;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("MenuView OnCreate()");
        }
    }
}
