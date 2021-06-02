using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Foundation;
using SkyDrop.Core.Services;

namespace SkyDrop.iOS.Services
{
    public class NSUrlHttpClientFactory : ISkyDropHttpClientFactory
    {
        private HttpClient singletonInstance;
        
        public HttpClient GetSkyDropHttpClientInstance()
        {
            // Re-use one HttpClient instance if already exists
            if (singletonInstance != null)
                return singletonInstance;
            
            // Create a background configuration for the application, enables background upload or download
            var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration ("skydrop.skydrop");
            
            var client = new HttpClient(new NSUrlSessionHandler(configuration))
            {
                BaseAddress = new Uri("https://siasky.net/"), 
                Timeout = TimeSpan.FromMinutes(120)
            };
            
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            singletonInstance = client;
            
            return client;
        }
    }
}