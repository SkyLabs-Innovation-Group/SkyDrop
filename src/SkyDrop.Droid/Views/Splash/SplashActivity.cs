using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.Platforms.Android.Views;
using SkyDrop.Core.DataModels;
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
        public static SkyFile SkyFileInput;
        public static EventHandler<SkyFile> NewSkyFileInput;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            HandleInputFile();
        }

        /// <summary>
        /// Handle file when SkyDrop is selected from share menu
        /// </summary>
        private void HandleInputFile()
        {
            try
            {
                var startIntent = Intent;
                if (startIntent.Action != Intent.ActionSend)
                    return;

                var parcel = (IParcelable)startIntent.GetParcelableExtra(Intent.ExtraStream);
                var uri = (Android.Net.Uri)parcel;
                var uriString = uri.ToString();

                MimeTypeMap mime = MimeTypeMap.Singleton;
                string fileExtension = mime.GetExtensionFromMimeType(ContentResolver.GetType(uri));
                var skyFile = new SkyFile
                {
                    FullFilePath = uriString,
                    Filename = $"{Guid.NewGuid()}.{fileExtension}"
                };

                var stream = ContentResolver.OpenInputStream(uri);
                skyFile.FileSizeBytes = stream.Length;
                skyFile.AndroidContentStream = stream;

                SkyFileInput = skyFile;
                NewSkyFileInput?.Invoke(this, skyFile);
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "Failed to load file", ToastLength.Short).Show();
            }
        }
    }
}
