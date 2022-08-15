using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using MvvmCross;
using Newtonsoft.Json;
using Realms;
using SkyDrop.Core.Services;
using Xamarin.Essentials;

namespace SkyDrop.Core.DataModels
{
    public class SkynetPortal
    {
        private static SkynetPortal _selectedPortalInstance;
        public static SkynetPortal SelectedPortal { get => _selectedPortalInstance ?? GetSelectedSkynetPortal(); set => _selectedPortalInstance = SetSelectedSkynetPortal(value); }

        private static SkynetPortal GetSelectedSkynetPortal()
        {
            if (_selectedPortalInstance != null)
                return _selectedPortalInstance;

            string portalUrl = Preferences.Get(PreferenceKey.SelectedSkynetPortal, "");

            if (string.IsNullOrEmpty(portalUrl))
                return SiaskyPortal;
            else
            {
                var portal = new SkynetPortal(portalUrl);
                portal.UserApiToken = SecureStorage.GetAsync(portal.GetApiTokenPrefKey()).GetAwaiter().GetResult();

                if (portal.HasApiToken())
                {
                    var httpClientFactory = Mvx.IoCProvider.GetSingleton<ISkyDropHttpClientFactory>();
                    httpClientFactory.UpdateHttpClientWithNewToken(portal);
                }

                return portal;
            }
        }

        private static SkynetPortal SetSelectedSkynetPortal(SkynetPortal portal)
        {
            if (string.IsNullOrEmpty(portal.ToString()))
                return SiaskyPortal;

            Preferences.Remove(PreferenceKey.SelectedSkynetPortal);
            Preferences.Set(PreferenceKey.SelectedSkynetPortal, portal.ToString());

            if (portal.HasApiToken())
            {
                string key = portal.GetApiTokenPrefKey();
                SecureStorage.Remove(key);
                SecureStorage.SetAsync(key, portal.UserApiToken).GetAwaiter().GetResult();

                var httpClientFactory = Mvx.IoCProvider.GetSingleton<ISkyDropHttpClientFactory>();
                httpClientFactory.UpdateHttpClientWithNewToken(portal);

                var ffImageService = Mvx.IoCProvider.GetSingleton<IFFImageService>();
                ffImageService.UpdateHttpClient(httpClientFactory.GetSkyDropHttpClientInstance(portal));
            }
            return portal;
        }

        public const string SiaskyPortalUrl = "https://siasky.net";

        public const string SkyportalXyzUrl = "https://skyportal.xyz";
        public string GetApiTokenPrefKey()
        {
            return $"{PreferenceKey.PrefixPortalApiToken}{BaseUrl}";
        }

        
        public static SkynetPortal SiaskyPortal = new SkynetPortal(SiaskyPortalUrl);
        
        public static SkynetPortal SkyportalXyz = new SkynetPortal(SkyportalXyzUrl);

        public SkynetPortal(string baseUrl)
        {
            this.BaseUrl = baseUrl;
            this.InitialBaseUrl = baseUrl;
        }
        
        // Used for Fody.Weaver
        public string BaseUrl { get; set; }

        public string UserApiToken { get; set; }

        public bool HasApiToken() => !string.IsNullOrEmpty(UserApiToken);

        public readonly string InitialBaseUrl;

        public override int GetHashCode()
        {
            return InitialBaseUrl.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SkynetPortal portal))
                return false;
            
            else
                return portal.BaseUrl == this.BaseUrl;
        }

        public override string ToString() => BaseUrl;
    }
}