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
using MvvmCross.Commands;
using SkyDrop.Core.ViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Views.Files;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class ContactsView : BaseActivity<ContactsViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.ContactsView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ViewModel.CloseKeyboardCommand = new MvxCommand(() => this.HideKeyboard());
        }
    }
}
