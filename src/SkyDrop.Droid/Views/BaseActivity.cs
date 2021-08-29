using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services.Api;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Helper;
using System;

namespace SkyDrop.Droid.Views
{
    public abstract class BaseActivity<TViewModel> : MvxActivity<TViewModel>
        where TViewModel : class, IMvxViewModel
    {
        private ILog _log;
        public ILog Log => _log ??= (ViewModel as BaseViewModel)?.Log;

        protected abstract int ActivityLayoutId { get; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(ActivityLayoutId);


            AndroidUtil.CreateNotificationChannel(this);
        }

      

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
