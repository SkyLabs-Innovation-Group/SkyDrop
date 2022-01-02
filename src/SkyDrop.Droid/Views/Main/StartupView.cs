using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Webkit;
using SkyDrop.Core.DataModels;
using Android.Views;
using Android.Content.PM;
using MvvmCross;
using MvvmCross.Navigation;
using SkyDrop.Core.ViewModels.Main;
using Android.Widget;
using MvvmCross.Platforms.Android.Core;
using System.Threading.Tasks;
using MvvmCross.Core;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android.Views;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    //[IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeTypes = new[] { "image/*", "video/*", "audio/*", "application/*" })]
    public class StartupView : Activity
    {
        private TextView textView;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                //RegisterDefaultSetupDependencies()
                //MvxSetup.Instance().ioc
                // MvxAndroidSetup.Instance().InitializePrimary();
                //MvxAndroidSetup.Instance().InitializeSecondary();

                //Xamarin.Essentials.Platform.Init(this, bundle);
                /*
                var setupInstance = new Setup();
                setupInstance.PlatformInitialize(ApplicationContext);
                setupInstance.InitializePrimary();
                setupInstance.InitializeSecondary();

                var instance = MvxIoCProvider.Initialize();
                */
                //var setupSingleton = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
                //setupSingleton.EnsureInitialized();

                var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
                setup.EnsureInitialized();

                //if (MvxAndroidSetupSingleton.Instance == null)
                //    throw new Exception("MvxAndroidSetupSingleton.Instance was null");

                //MvxAndroidSetupSingleton.Instance.EnsureInitialized();
                /*
                var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
                if (setup == null)
                    throw new Exception("Setup was null");
                //setup.InitializeAndMonitor(null);
                setup.EnsureInitialized();*/
            }
            catch (Exception e)
            {
                PrintLater(e.Message, 3000);
            }

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.StartupView);
            textView = FindViewById<TextView>(Resource.Id.startup_textview);

            HandleInputFile();
        }

        /// <summary>
        /// Handle file when SkyDrop is selected from share menu
        /// </summary>
        private void HandleInputFile()
        {
            try
            {
                if (Intent.Action != Intent.ActionSend)
                    return;

                if (Mvx.IoCProvider == null)
                    throw new Exception("Mvx.IoCProvider is null");
                
                var navigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
                if (navigationService == null)
                    throw new Exception("navigationService is null");

                navigationService.Navigate<DropViewModel>();
                /*
                var parcel = (IParcelable)Intent.GetParcelableExtra(Intent.ExtraStream);
                var uri = (Android.Net.Uri)parcel;
                var uriString = uri.ToString();

                MimeTypeMap mime = MimeTypeMap.Singleton;
                string fileExtension = mime.GetExtensionFromMimeType(ContentResolver.GetType(uri));
                var skyFile = new SkyFile
                {
                    FullFilePath = uriString,
                    Filename = $"{Guid.NewGuid()}.{fileExtension}"
                };

                using var stream = skyFile.GetStream();
                skyFile.FileSizeBytes = stream.Length;
                */
                //ViewModel.StageFiles(new System.Collections.Generic.List<SkyFile> { skyFile }, false);
            }
            catch (Exception e)
            {
                Print(e.Message);
                //ViewModel.Log.Exception(e);
                //ViewModel.UserDialogs.Toast("Failed to load file");
            }
        }

        private void Print(string text)
        {
            textView.Text += $"{text}\n";
        }

        private async Task PrintLater(string text, int delay)
        {
            await Task.Delay(delay);
            Print(text);
        }
    }
}
