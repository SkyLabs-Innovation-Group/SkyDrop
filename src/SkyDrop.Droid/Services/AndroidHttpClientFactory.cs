using System;
using System.Net.Http;
using System.Net.Http.Headers;
using SkyDrop.Core;
using SkyDrop.Core.Components;
using SkyDrop.Core.DataModels;
using SkyDrop.Droid.Helper;
using Xamarin.Android.Net;
using Xamarin.Essentials;

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
            //re-use HttpClient if already created
            if (HttpClientsPerPortal.ContainsKey(portal))
                return HttpClientsPerPortal[portal];

            HttpClient client = null; // todo: re-enable secure message handler on android when we can
            //if (Preferences.Get(PreferenceKey.RequireSecureConnection, true))
            //{
            //    //normal SSL certificate verification
            //    client = new HttpClient(new ManagedRetryHandler())
            //    {
            //        BaseAddress = new Uri(portal.BaseUrl),
            //    };
            //}
            //else
            //{
                //don't verify SSL certificates
                var handler = GetInsecureMessageHandler();
                client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(portal.BaseUrl),
                };
            //}

            if (portal.HasApiToken())
                AddApiTokenHeader(client, portal);

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            //save the HttpClient for efficient re-use
            HttpClientsPerPortal.Add(portal, client);

            return client;
        }

        private ManagedRetryHandler GetInsecureMessageHandler()
        {
            var handler = new ManagedRetryHandler();

            //accept all SSL certificates (insecure!)
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        }
    }
}