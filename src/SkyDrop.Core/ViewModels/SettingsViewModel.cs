using System;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;
using Xamarin.Essentials;

namespace SkyDrop.Core.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IEncryptionService encryptionService;

        private readonly ISkyDropHttpClientFactory httpClientFactory;
        private readonly IMvxNavigationService navigationService;

        public SettingsViewModel(ISingletonService singletonService,
            ISkyDropHttpClientFactory skyDropHttpClientFactory,
            IMvxNavigationService navigationService,
            IEncryptionService encryptionService) : base(singletonService)
        {
            httpClientFactory = skyDropHttpClientFactory;
            this.navigationService = navigationService;
            this.encryptionService = encryptionService;

            Title = "Settings";

            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
            SetDeviceNameCommand = new MvxCommand(SetDeviceName);
            ViewOnboardingCommand =
                new MvxAsyncCommand(async () => await navigationService.Navigate<OnboardingViewModel>());

            DeviceName = encryptionService.GetDeviceName();
        }

        public bool UploadNotificationsEnabled { get; set; } = true;
        public bool VerifySslCertificates { get; set; } = true;
        public string DeviceName { get; set; }

        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand SetDeviceNameCommand { get; set; }
        public IMvxCommand CloseKeyboardCommand { get; set; }
        public IMvxCommand ViewOnboardingCommand { get; set; }

        public override void ViewCreated()
        {
            UploadNotificationsEnabled = Preferences.Get(PreferenceKey.UploadNotificationsEnabled, true);
            VerifySslCertificates = Preferences.Get(PreferenceKey.RequireSecureConnection, true);
            base.ViewCreated();
        }

        public void SetUploadNotificationEnabled(bool value)
        {
            UploadNotificationsEnabled = value;
            Preferences.Remove(PreferenceKey.UploadNotificationsEnabled);
            Preferences.Set(PreferenceKey.UploadNotificationsEnabled, value);
        }

        public void SetVerifySslCertificates(bool value)
        {
            VerifySslCertificates = value;
            Preferences.Remove(PreferenceKey.RequireSecureConnection);
            Preferences.Set(PreferenceKey.RequireSecureConnection, value);

            //clients must be rebuilt after changing this setting
            httpClientFactory.ClearCachedClients();
        }

        public void Toast(string message)
        {
            SingletonService.UserDialogs.Toast(message);
        }

        public void Close()
        {
            navigationService.Close(this);
        }

        private void SetDeviceName()
        {
            try
            {
                encryptionService.UpdateDeviceName(DeviceName);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            finally
            {
                CloseKeyboardCommand.Execute();
            }
        }
    }
}