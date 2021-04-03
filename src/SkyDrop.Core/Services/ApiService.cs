using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
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

        public async Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes, CancellationTokenSource cancellationToken)
        {
            if (fileSizeBytes == 0)
                Log.Error("File size was zero when uploading file");

            var url = $"{Util.Portal}/skynet/skyfile";
            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(file), "file", filename);

            Log.Trace("Sending file " + filename);

            var response = await httpClient.PostAsync(url, form, cancellationToken.Token).ConfigureAwait(false);

            Log.Trace(response.RequestMessage.ToString());

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var skyFile = JsonConvert.DeserializeObject<SkyFile>(responseString);
            skyFile.Filename = filename;
            skyFile.Status = FileStatus.Uploaded;
            skyFile.FileSizeBytes = fileSizeBytes;

            return skyFile;
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes, CancellationTokenSource cancellationToken);
    }
}
