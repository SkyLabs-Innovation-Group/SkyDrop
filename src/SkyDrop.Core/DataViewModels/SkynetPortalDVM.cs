using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.Core.DataViewModels
{
    public class SkynetPortalDvm : MvxNotifyPropertyChanged
    {
        public SkynetPortalDvm(SkynetPortal portal)
        {
            Portal = portal;
            Name = portal.Name;
            BaseUrl = portal.BaseUrl;
            RealmId = portal.Id;
            PortalPreferencesPosition = portal.PortalPreferencesPosition;
            ApiTokenPrefKey = portal.GetApiTokenPrefKey();
        }

        public SkynetPortal Portal { get; set; }
        public string RealmId { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public int PortalPreferencesPosition { get; set; }
        public string ApiTokenPrefKey { get; set; }
        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand LongPressCommand { get; set; }
        public IMvxCommand MoveUpCommand { get; set; }
        public IMvxCommand MoveDownCommand { get; set; }

        public Task<string> GetApiToken()
        {
            return SecureStorage.GetAsync(ApiTokenPrefKey);
        }
    }
}