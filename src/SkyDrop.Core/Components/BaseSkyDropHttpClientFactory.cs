using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.Components
{
    public abstract class BaseSkyDropHttpClientFactory : ISkyDropHttpClientFactory
    {
        protected const string PortalApiTokenHeader = "Skynet-Api-Key";

        // todo: nb that this might one day cause issues if we stored too many HttpClient instances e.g. for 100s of portals
        protected Dictionary<SkynetPortal, HttpClient> HttpClientsPerPortal { get; private set; } = new Dictionary<SkynetPortal, HttpClient>();

        /// <summary>
        /// This is the default paramaterless implementation for GetSkyDropHttpClientInstance(), it returns the Siasky.net portal.
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance() => GetSkyDropHttpClientInstance(SkynetPortal.SiaskyPortal);

        /// <summary>
        /// This method returns a HttpClient connected to $portal.BaseUrl.
        /// </summary>
        public abstract HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal);

        public void ClearCachedClients()
        {
            HttpClientsPerPortal = new Dictionary<SkynetPortal, HttpClient>();
        }

        public void UpdateHttpClientWithNewToken(SkynetPortal portal)
        {
            var httpClient = GetSkyDropHttpClientInstance(portal);

            AddApiTokenHeader(httpClient, portal);
        }

        public string GetTokenForHttpClient(SkynetPortal portal) => GetTokenForHttpClient(GetSkyDropHttpClientInstance(portal));

        public string GetTokenForHttpClient(HttpClient client)
        {
            string apiTokenHeader = null;
            try
            {
                apiTokenHeader = client.DefaultRequestHeaders.Where(c => c.Key == PortalApiTokenHeader).FirstOrDefault().Value.FirstOrDefault();
            }
            catch (Exception ex) { }

            return apiTokenHeader;
        }

        protected static void AddApiTokenHeader(HttpClient client, SkynetPortal portal)
        {
            client.DefaultRequestHeaders.Remove(PortalApiTokenHeader);

            if (portal.HasApiToken())
            {
                client.DefaultRequestHeaders.Add(PortalApiTokenHeader, portal.UserApiToken);
            }
        }
    }
}