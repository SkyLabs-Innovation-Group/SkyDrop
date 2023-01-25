using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Foundation;
using ObjCRuntime;
using SkyDrop.Core.Components;
using SkyDrop.Core.DataModels;
using SkyDrop.iOS.Common;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Services
{
    public class NsUrlHttpClientFactory : BaseSkyDropHttpClientFactory
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
            // On simulator we use DefaultSessionConfiguration because background session causes upload to fail
            var configuration = Runtime.Arch == Arch.DEVICE
                ? NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(
                    $"skydrop.skydrop.{HttpClientsPerPortal.Count}")
                : NSUrlSessionConfiguration.DefaultSessionConfiguration;

            var client = new HttpClient(new RetryHandler(configuration))
            {
                BaseAddress = new Uri(portal.BaseUrl),
                Timeout = TimeSpan.FromMinutes(120)
            };

            portal.UserApiToken ??= SecureStorage.GetAsync(portal.GetApiTokenPrefKey()).GetAwaiter().GetResult();

            AddApiTokenHeader(client, portal);

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            // Save the HttpClient for efficient re-use
            HttpClientsPerPortal.Add(portal, client);

            return client;
        }
    }
}