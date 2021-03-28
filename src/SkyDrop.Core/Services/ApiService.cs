using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Http;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Services
{
    public class ApiService : IApiService
    {
        public ILog Log { get; }

        public event EventHandler<(long uploaded, long size)> UploadProgressUpdate;

        public ApiService(ILog log)
        {
            Log = log;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://siasky.net/");
        }

        private HttpClient httpClient { get; set; }

        //TODO: try using HttpClientRequest to disable request buffering
        //https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest.allowwritestreambuffering?redirectedfrom=MSDN&view=net-5.0#System_Net_HttpWebRequest_AllowWriteStreamBuffering
        public async Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes)
        {
            if (fileSizeBytes == 0)
                Log.Error("File size was zero when uploading file");

            var url = $"{Util.Portal}/skynet/skyfile";
            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(file), "file", filename);

            //convert form to ProgressStreamContent so we can track upload progress
            var requestContent = new ProgressStreamContent(form);
            requestContent.ProgressUpdate += UploadProgressUpdate;

            Log.Trace("Sending file " + filename);

            var response = await httpClient.PostAsync(url, requestContent).ConfigureAwait(false);

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
        event EventHandler<(long uploaded, long size)> UploadProgressUpdate;

        Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes);
    }
}
