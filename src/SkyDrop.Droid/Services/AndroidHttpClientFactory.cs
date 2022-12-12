using System;
using System.Net.Http;
using System.Net.Http.Headers;
using SkyDrop.Core;
using SkyDrop.Core.Components;
using SkyDrop.Core.DataModels;
using Xamarin.Android.Net;
using Xamarin.Essentials;

namespace SkyDrop.Droid.Services
{
    public class AndroidHttpClientFactory : BaseSkyDropHttpClientFactory
    {
        // Check BaseSkyDropHttpClientFactory for the default portal logic.

        /// <summary>
        ///     Get the HttpClient which connects to the portal provided by argument.
        /// </summary>
        public override HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal)
        {
            //re-use HttpClient if already created
            if (HttpClientsPerPortal.ContainsKey(portal))
                return HttpClientsPerPortal[portal];

            HttpClient client;
            if (Preferences.Get(PreferenceKey.RequireSecureConnection, true))
            {
                //normal SSL certificate verification
                client = new HttpClient(new AndroidClientHandler())
                {
                    BaseAddress = new Uri(portal.BaseUrl),
                    Timeout = TimeSpan.FromMinutes(120)
                };
            }
            else
            {
                //don't verify SSL certificates
                var handler = GetInsecureClientHandler();
                client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(portal.BaseUrl),
                    Timeout = TimeSpan.FromMinutes(120)
                };
            }

            if (portal.HasApiToken())
                AddApiTokenHeader(client, portal);

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            //save the HttpClient for efficient re-use
            HttpClientsPerPortal.Add(portal, client);

            return client;
        }

        private AndroidClientHandler GetInsecureClientHandler()
        {
            var handler = new AndroidClientHandler();

            //accept all SSL certificates (insecure!)
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        }
    }
}