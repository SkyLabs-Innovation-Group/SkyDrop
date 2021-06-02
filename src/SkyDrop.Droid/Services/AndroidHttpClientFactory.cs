using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.Services;

namespace SkyDrop.Droid.Services
{
    public class AndroidHttpClientFactory : ISkyDropHttpClientFactory
    {
        private HttpClient singletonInstance;
        
        public HttpClient GetSkyDropHttpClientInstance()
        {
            // Re-use one HttpClient instance if already exists
            if (singletonInstance != null)
                return singletonInstance;
            
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://siasky.net/"), 
                Timeout = TimeSpan.FromMinutes(120),
            };
            
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            
            singletonInstance = client;
            
            return client;
        }
    }
}