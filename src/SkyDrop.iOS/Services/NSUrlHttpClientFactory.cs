using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Foundation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.iOS.Services
{
    public class NSUrlHttpClientFactory : ISkyDropHttpClientFactory
    {
        /// <summary>
        /// Maintaining this list might not be required, I have added it for if we want to only support whitelisted portals.
        /// </summary>
        private List<SkynetPortal> SupportedPortals { get; } = new List<SkynetPortal>() { SkynetPortal.SiaskyPortal, SkynetPortal.SkyportalXyz };
        
        // todo: nb that this might one day cause issues if we stored too many HttpClient instances e.g. for 100s of portals
        private Dictionary<SkynetPortal, HttpClient> HttpClientsPerPortal { get; } = new Dictionary<SkynetPortal, HttpClient>();
        
        /// <summary>
        /// Get the HttpClient which connects to the default Siasky portal.
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance()
        {
            return GetSkyDropHttpClientInstance(SkynetPortal.SkyportalXyz);
        }

        /// <summary>
        /// Get the HttpClient which connects to the portal provided by argument..
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal)
        {
            // Re-use HttpClient if already created
            if (HttpClientsPerPortal.ContainsKey(portal))
                return HttpClientsPerPortal[portal];
            
            // Create a background configuration for the application, enables background upload or download
            var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration ("skydrop.skydrop");
            
            var client = new HttpClient(new NSUrlSessionHandler(configuration))
            {
                BaseAddress = new Uri(portal.BaseUrl), 
                Timeout = TimeSpan.FromMinutes(120)
            };
            
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            // Save the HttpClient for efficient re-use
            HttpClientsPerPortal.Add(portal, client);
            
            return client;
        }
        
    }
}