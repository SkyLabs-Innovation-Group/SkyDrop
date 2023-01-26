using Android.App;
using MvvmCross.Platforms.Android.Views;

namespace SkyDrop.Droid.Views.Splash
{
    [Activity(
        NoHistory = true,
        MainLauncher = true,
        Label = "@string/app_name",
        Theme = "@style/AppTheme.Splash",
        Icon = "@mipmap/ic_launcher",
        RoundIcon = "@mipmap/ic_launcher_round", Exported = true)]
    public class SplashActivity : MvxSplashScreenActivity
    {
    }
}