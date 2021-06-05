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
            
            // When first portal instance is created, it will be set here. This would start to cause bugs potentially if we accidentally created a
            // SkynetPortal instance on startup for some other reason (e.g. to check equality in a startup method Initialize())
            if (isFirstPortal) SelectedPortal = this;
            isFirstPortal = false;
        }
        
        public string BaseUrl { get; }

        public override int GetHashCode()
        {
            return BaseUrl?.GetHashCode() ?? 0;
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