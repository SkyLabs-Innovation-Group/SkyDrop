using System;
using System.IO;
using System.IO.Compression;
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
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Sia-Agent");
        }

        private HttpClient httpClient { get; set; }


        public async Task<byte[]> GetBytesFromSkylink(string skylink)
        {
            try
            {
                var fileBytes = await httpClient.GetByteArrayAsync(skylink);

                return fileBytes;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Can get content type, file name, and content size using this query.
        /// </summary>
        /// <param name="skylink"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetMetadataForSkylink(string skylink)
        {
            try
            {
                var headRequestMessage = new HttpRequestMessage(HttpMethod.Head, skylink);

                var headResponse = await httpClient.SendAsync(headRequestMessage);

                return headResponse;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                throw;
            }
        }

        public async Task<byte[]> DownloadSkyFileBytes(string skylink)
        {
            if (string.IsNullOrEmpty(skylink))
                throw new ArgumentNullException(nameof(skylink));

            bool isZip = false;
            try
            {
                var headResponse = await GetMetadataForSkylink(skylink);

                if (headResponse.ToString()
                    .Contains("Content-Type: application/zip"))
                    isZip = true;

                if (isZip) // TODO UI option to unzip
                {
                    using var memoryStream = new MemoryStream();

                    using (var stream = await httpClient.GetStreamAsync(skylink))
                    {
                        // Copying the stream is here a bit slow, but that's ok if it's optional behind a button when receiving,
                        // e.g. "would you like extract the zip to a directory?"
                        await stream.CopyToAsync(memoryStream);
                    }

                    var zipArchive = new ZipArchive(memoryStream);

                    var tempPath = Path.GetTempPath();

                    // Add option to rename here
                    var zipArchivePath = Path.Combine(tempPath, "sky_archive.zip");

                    zipArchive.ExtractToDirectory(zipArchivePath);

                    // We might not need to return bytes for receiving zips, if extracting to directory already handles that.
                    // I'll refactor it after we decide what we want the behaviour to be. For now you can test it works in debugging,

                    // throw new NotImplementedException();
                }

                return await DownloadSkyFileBytes(skylink);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                throw;
            }
        }

        public async Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes,
            CancellationTokenSource cancellationToken)
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
        Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes,
            CancellationTokenSource cancellationToken);

        Task<byte[]> DownloadSkyFileBytes(string skylink);

        Task<byte[]> GetBytesFromSkylink(string skylink);

        Task<HttpResponseMessage> GetMetadataForSkylink(string skylink);
    }
}