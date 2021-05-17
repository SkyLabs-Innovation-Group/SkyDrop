using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fody;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Services
{
    [ConfigureAwait(false)]
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

        public async Task<SkyFile> UploadFile(SkyFile skyfile, CancellationTokenSource cancellationToken)
        {
            var fileSizeBytes = skyfile.FileSizeBytes;
            var filename = skyfile.Filename;

            var url = $"{Util.Portal}/skynet/skyfile";
            var form = new MultipartFormDataContent();
            
            using var file = skyfile.GetStream();
            
            if (fileSizeBytes == 0)
                Log.Error("File size was zero when uploading file");

            form.Add(new StreamContent(file), "file", filename);

            Log.Trace("Sending file " + filename);

            var response = await httpClient.PostAsync(url, form, cancellationToken.Token);

            Log.Trace(response.RequestMessage.ToString());

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var skyFile = JsonConvert.DeserializeObject<SkyFile>(responseString);
            skyFile.Filename = filename;
            skyFile.Status = FileStatus.Uploaded;
            skyFile.FileSizeBytes = fileSizeBytes;

            return skyFile;
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(SkyFile skyFile, CancellationTokenSource cancellationToken);
    }
}
