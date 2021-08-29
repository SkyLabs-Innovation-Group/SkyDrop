using System.Net.Http;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services.Api
{
    public interface ISkyDropHttpClientFactory
    {
        HttpClient GetSkyDropHttpClientInstance();
        
        HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal);
    }
}