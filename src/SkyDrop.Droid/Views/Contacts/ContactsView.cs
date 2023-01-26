using Android.App;
using Android.OS;
using Android.Views;
using MvvmCross.Commands;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, Exported = true)]
    public class ContactsView : BaseActivity<ContactsViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.ContactsView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ViewModel.CloseKeyboardCommand = new MvxAsyncCommand(this.HideKeyboard);
        }
    }
}