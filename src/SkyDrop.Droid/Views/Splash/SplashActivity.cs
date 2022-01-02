using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.Platforms.Android.Views;
using SkyDrop.Core.ViewModels.Main;

namespace SkyDrop.Droid.Views.Splash
{
    [Activity(
        NoHistory = true,
        MainLauncher = true,
        Label = "@string/app_name",
        Theme = "@style/AppTheme.Splash",
        Icon = "@mipmap/ic_launcher",
        RoundIcon = "@mipmap/ic_launcher_round")]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeTypes = new[] { "image/*", "video/*", "audio/*", "application/*" })]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public static Intent StartupIntent;
        public static EventHandler<bool> NewIntent;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StartupIntent = Intent;
            NewIntent?.Invoke(this, true);
        }
    }
}
