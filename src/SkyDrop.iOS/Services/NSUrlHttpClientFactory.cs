using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Foundation;
using SkyDrop.Core.Components;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.iOS.Services
{
    public class NSUrlHttpClientFactory : BaseSkyDropHttpClientFactory
    {  
        // Check BaseSkyDropHttpClientFactory for the default portal logic.
        
        /// <summary>
        /// Get the HttpClient which connects to the portal provided by argument..
        /// </summary>
        public override HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal)
        {
            // Re-use HttpClient if already created
            if (HttpClientsPerPortal.ContainsKey(portal))
                return HttpClientsPerPortal[portal];

            // Create a background configuration for the application, enables background upload or download
            var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration ($"skydrop.skydrop.{HttpClientsPerPortal.Count}");

            
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