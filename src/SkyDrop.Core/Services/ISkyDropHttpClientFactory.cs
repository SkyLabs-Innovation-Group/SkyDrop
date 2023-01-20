using System.Net.Http;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    public interface ISkyDropHttpClientFactory
    {
        HttpClient GetSkyDropHttpClientInstance();

        HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal);

        void CancelAllUploadsForClient(SkynetPortal portal);

        void ClearCachedClients();

        void UpdateHttpClientWithNewToken(SkynetPortal portal);

        string GetTokenForHttpClient(SkynetPortal portal);

        string GetTokenForHttpClient(HttpClient client);
    }
}