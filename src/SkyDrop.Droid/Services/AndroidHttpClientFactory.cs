using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.Components;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Droid.Services
{
    public class AndroidHttpClientFactory : BaseSkyDropHttpClientFactory
    {
        // Check BaseSkyDropHttpClientFactory for the default portal logic.

        /// <summary>
        /// Get the HttpClient which connects to the portal provided by argument.
        /// </summary>
        public override HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal)
        {
            // Re-use HttpClient if already created
            if (HttpClientsPerPortal.ContainsKey(portal))
                return HttpClientsPerPortal[portal];
            
            var client = new HttpClient()
            {
                BaseAddress = new Uri(portal.BaseUrl), 
                Timeout = TimeSpan.FromMinutes(120),
            };
            
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            // Save the HttpClient for efficient re-use
            HttpClientsPerPortal.Add(portal, client);
            
            return client;
        }
    }
}