using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json;
using Realms;

namespace SkyDrop.Core.DataModels
{
    public class SkynetPortal
    {
        public static SkynetPortal SelectedPortal { get; set; }
        
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