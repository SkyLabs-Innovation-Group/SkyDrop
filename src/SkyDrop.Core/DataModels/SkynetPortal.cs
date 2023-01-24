using System;
using System.Linq;
using MvvmCross;
using Realms;
using SkyDrop.Core.Services;
using Xamarin.Essentials;

namespace SkyDrop.Core.DataModels
{
    public class SkynetPortal : RealmObject
    {
        public const string DefaultWeb3PortalUrl = "https://web3portal.com";

        private static SkynetPortal _selectedPortalInstance;

        public SkynetPortal()
        {
        }

        public SkynetPortal(string baseUrl)
        {
            BaseUrl = baseUrl;
            InitialBaseUrl = baseUrl;
            Id = Guid.NewGuid().ToString();
        }

        public SkynetPortal(string baseUrl, string name) : this(baseUrl)
        {
            Name = name;
        }

        public string InitialBaseUrl { get; set; }

        public static SkynetPortal SelectedPortal
        {
            get => _selectedPortalInstance ?? GetSelectedSkynetPortal();
            set => _selectedPortalInstance = SetSelectedSkynetPortal(value);
        }

        [PrimaryKey] public string Id { get; set; }

        public int PortalPreferencesPosition { get; set; }

        public string Name { get; set; }

        // Used for Fody.Weaver
        public string BaseUrl { get; set; }

        public string UserApiToken { get; set; }

        public bool HasLoggedInBrowser { get; set; }

        private static SkynetPortal GetSelectedSkynetPortal()
        {
            if (_selectedPortalInstance != null)
                return _selectedPortalInstance;

            var portalUrl = Preferences.Get(PreferenceKey.SelectedSkynetPortal, "");

            if (string.IsNullOrEmpty(portalUrl)) return new SkynetPortal(DefaultWeb3PortalUrl);

            var portal = new SkynetPortal(portalUrl);
            portal.UserApiToken = SecureStorage.GetAsync(portal.GetApiTokenPrefKey()).GetAwaiter().GetResult();

            if (portal.HasApiToken())
            {
                var httpClientFactory = Mvx.IoCProvider.GetSingleton<ISkyDropHttpClientFactory>();
                httpClientFactory.UpdateHttpClientWithNewToken(portal);
            }

            return portal;
        }

        public static string GetLoginUrl(string url)
        {
            //remove protocol
            if (url.StartsWith("https://")) url = url.Substring(8);

            return $"https://account.{url}";
        }

        public static bool IsValidUri(string uriString)
        {
            if (!Uri.TryCreate(uriString, UriKind.Absolute, out Uri uri))
            {
                return false;
            }
            return true;
        }

        private static SkynetPortal SetSelectedSkynetPortal(SkynetPortal portal)
        {
            if (string.IsNullOrEmpty(portal.ToString()))
                return new SkynetPortal(DefaultWeb3PortalUrl);

            var storageService = Mvx.IoCProvider.Resolve<IStorageService>();

            if (!IsValidUri(portal.BaseUrl))
            {
                Realm.GetInstance().Write(() =>
                {
                    portal.BaseUrl = "http://" + portal.BaseUrl;
                    portal.BaseUrl = Uri.EscapeUriString(portal.BaseUrl);
                    Realm.GetInstance().Add(portal);
                });

                if (!IsValidUri(portal.BaseUrl))
                    return storageService.LoadSkynetPortals().Where(p => p.BaseUrl.Contains(DefaultWeb3PortalUrl)).FirstOrDefault();
            }

            Preferences.Remove(PreferenceKey.SelectedSkynetPortal);
            Preferences.Set(PreferenceKey.SelectedSkynetPortal, portal.ToString());

            storageService.SaveSkynetPortal(portal);

            if (portal.HasApiToken())
            {
                var key = portal.GetApiTokenPrefKey();
                SecureStorage.Remove(key);
                SecureStorage.SetAsync(key, portal.UserApiToken).GetAwaiter().GetResult();

                var httpClientFactory = Mvx.IoCProvider.GetSingleton<ISkyDropHttpClientFactory>();
                httpClientFactory.UpdateHttpClientWithNewToken(portal);

                var ffImageService = Mvx.IoCProvider.GetSingleton<IFfImageService>();
                ffImageService.UpdateHttpClient(httpClientFactory.GetSkyDropHttpClientInstance(portal));
            }

            return portal;
        }

        public string GetApiTokenPrefKey()
        {
            return $"{PreferenceKey.PrefixPortalApiToken}{BaseUrl}".ToLowerInvariant();
        }

        public bool HasApiToken()
        {
            return !string.IsNullOrEmpty(UserApiToken);
        }

        public override int GetHashCode()
        {
            return InitialBaseUrl.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SkynetPortal portal))
                return false;

            return portal.BaseUrl == BaseUrl;
        }

        public override string ToString()
        {
            return BaseUrl;
        }
    }
}