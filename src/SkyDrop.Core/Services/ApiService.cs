using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Services
{
    public class ApiService : IApiService
    {
        public ILog Log { get; }

        public ApiService(ILog log)
        {
            Log = log;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://siasky.net/");
        }

        private HttpClient httpClient { get; set; }

        public async Task<SkyFile> UploadFile(string filename, byte[] file)
        {
            var url = $"{Util.Portal}/skynet/skyfile";
            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(file), "file", filename);

            Log.Trace("Sending file " + filename);

            var response = await httpClient.PostAsync(url, form).ConfigureAwait(false);

            Log.Trace(response.RequestMessage.ToString());

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var skyfile = JsonConvert.DeserializeObject<SkyFile>(responseString);
            skyfile.Filename = filename;

            return skyfile;
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(string filename, byte[] file);
    }
}
