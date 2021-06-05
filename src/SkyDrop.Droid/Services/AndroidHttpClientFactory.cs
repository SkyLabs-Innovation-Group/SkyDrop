using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Droid.Services
{
    public class AndroidHttpClientFactory : ISkyDropHttpClientFactory
    {
        private HashSet<SkynetPortal> SupportedPortals { get; } = new HashSet<SkynetPortal>() { SkynetPortal.SiaskyPortal };
        
        // todo: nb that this might one day cause issues if we stored too many HttpClient instances e.g. for 100s of portals
        private Dictionary<SkynetPortal, HttpClient> HttpClientsPerPortal { get; } = new Dictionary<SkynetPortal, HttpClient>();

        /// <summary>
        /// Get the HttpClient which connects to the default Siasky portal.
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance()
        {
            return GetSkyDropHttpClientInstance(SkynetPortal.SiaskyPortal);
        }

        /// <summary>
        /// Get the HttpClient which connects to the portal provided by argument.
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal)
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