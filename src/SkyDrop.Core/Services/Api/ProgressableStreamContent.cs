using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkyDrop.Core.Services.Api
{
    public class ProgressableStreamContent : HttpContent
    {
        private const long BytesInMegabyte = 1_048_576;
        private const int defaultBufferSize = 4096;

        private Stream content;
        private int bufferSize;
        private bool contentConsumed;
        private readonly long fileSizeBytes;

        private long uploadedBytesCount;
        private int uploadedMegabytesCount;
        private int uploadedPercentageCount;

        public EventHandler<int> ReportUploadedMegabytes;
        public EventHandler<int> ReportUploadedPercentage;
        public EventHandler<bool> ReportUploadDidComplete;

        public ProgressableStreamContent(Stream content, long fileSizeBytes, int bufferSize = defaultBufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            
            if (fileSizeBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(fileSizeBytes));

            this.fileSizeBytes = fileSizeBytes;
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            this.bufferSize = bufferSize;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Contract.Assert(stream != null);

            PrepareContent();

            return Task.Run(() =>
            {
                var buffer = new Byte[this.bufferSize];
                var uploaded = 0;
                
                using (content)
                    while (true)
                    {
                        var length = content.Read(buffer, 0, buffer.Length);
                        if (length <= 0) break;

                        uploaded += length;
                        ReportProgress(uploaded);

                        stream.Write(buffer, 0, length);
                    }

                ReportUploadDidComplete.Invoke(this, true);
                //downloader.ChangeState(DownloadState.PendingResponse);
            });
        }

        private void ReportProgress(long bytesCount)
        {
            uploadedBytesCount = bytesCount;

            int mbUploaded = (int)(uploadedBytesCount / BytesInMegabyte);
            if (mbUploaded > uploadedMegabytesCount)
            {
                uploadedMegabytesCount = mbUploaded;
                ReportUploadedMegabytes.Invoke(this, uploadedMegabytesCount);
            }
            
            int percentageUploaded = (int) (100 * (uploadedBytesCount / fileSizeBytes));
            percentageUploaded = Math.Min(percentageUploaded, 100);
            if (percentageUploaded > uploadedPercentageCount)
            {
                uploadedPercentageCount = percentageUploaded;
                ReportUploadedPercentage.Invoke(this, uploadedPercentageCount);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                content.Dispose();
            }

            base.Dispose(disposing);
        }


        private void PrepareContent()
        {
            if (contentConsumed)
            {
                // If the content needs to be written to a target stream a 2nd time, then the stream must support
                // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
                // stream (e.g. a NetworkStream).
                if (content.CanSeek)
                {
                    content.Position = 0;
                }
                else
                {
                    throw new InvalidOperationException("SR.net_http_content_stream_already_read");
                }
            }

            contentConsumed = true;
        }
    }
}