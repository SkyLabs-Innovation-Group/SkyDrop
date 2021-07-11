using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json;
using Realms;
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

            string portalUrl = Preferences.Get("selected_skynetportal", "");

            if (!string.IsNullOrEmpty(portalUrl))
                return new SkynetPortal(portalUrl);
            else // No saved portal => use default siasky.net/
                return SiaskyPortal;
        }

        private static SkynetPortal SetSelectedSkynetPortal(SkynetPortal portal)
        {
            Preferences.Set("selected_skynetportal", portal?.ToString());
            return portal;
        }

        public const string SiaskyPortalUrl = "https://siasky.net/";

        public const string SkyportalXyzUrl = "https://skyportal.xyz/";
        
        public static SkynetPortal SiaskyPortal = new SkynetPortal(SiaskyPortalUrl);
        
        public static SkynetPortal SkyportalXyz = new SkynetPortal(SkyportalXyzUrl);



        private bool isFirstPortal = true;
        public SkynetPortal(string baseUrl)
        {
            this.BaseUrl = baseUrl;
            this.InitialBaseUrl = baseUrl;
            
            // When first portal instance is created, it will be selected here. This would start to cause bugs potentially if we accidentally created a
            // SkynetPortal instance on startup for some other reason (e.g. to check equality in a startup method Initialize())
            if (isFirstPortal) SelectedPortal = this;
            isFirstPortal = false;
        }
        
        // Used for Fody.Weaver
        public string BaseUrl { get; set; }

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