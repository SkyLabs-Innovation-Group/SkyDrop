using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Realms;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    /// <summary>
    /// Keeps a running average of upload rates
    /// This is used to calculate estimated upload progress
    /// </summary>
    public class UploadTimerService : IUploadTimerService
    {
        private readonly ILog log;
        private readonly IStorageService storageService;
        private Stopwatch stopwatch;
        private Timer timer;
        private TimeSpan estimatedUploadTime;
        private long fileSizeBytes;

        public UploadTimerService(ILog log,
                                  IStorageService storageService)
        {
            this.log = log;
            this.storageService = storageService;
        }

        public void AddReading(TimeSpan time, long fileSizeBytes)
        {
            var uploadRate = (fileSizeBytes * 8) / time.TotalSeconds; //in bits/second

            //add the new value to the average
            var currentAverage = storageService.GetAverageUploadRate();
            var newDataPointCount = currentAverage.DataPointCount + 1;
            var newAverage = new UploadAverage
            {
                DataPointCount = newDataPointCount,
                Value = (currentAverage.Value * currentAverage.DataPointCount + uploadRate) / newDataPointCount
            };

            log.Trace($"New upload rate: {uploadRate}b/s");
            log.Trace($"New average upload rate: {newAverage.Value}b/s");

            //save new average
            storageService.SetAverageUploadRate(newAverage);
        }

        public TimeSpan EstimateUploadTime(long fileSizeBytes)
        {
            long fileSizeBits = fileSizeBytes * 8;
            var currentAverage = storageService.GetAverageUploadRate();
            var averageUploadSpeed = currentAverage.Value;
            if (averageUploadSpeed == 0)
                averageUploadSpeed = 800_000; //default 800kb/s speed

            var secondsPerBit = 1 / averageUploadSpeed;
            var extraSeconds = 5; //let's add some time to play it safe (overestimate for user satisfaction)
            var estimatedSeconds = fileSizeBits * secondsPerBit + extraSeconds;

            return TimeSpan.FromSeconds(estimatedSeconds);
        }

        public void StartUploadTimer(long fileSizeBytes, Action timerUpdateCallback)
        {
            this.fileSizeBytes = fileSizeBytes;
            estimatedUploadTime = EstimateUploadTime(fileSizeBytes);

            stopwatch = new Stopwatch();
            timer = new Timer();
            timer.Elapsed += (s, e) => timerUpdateCallback();
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Start();
            stopwatch.Start();
            timerUpdateCallback();
        }

        public void StopUploadTimer()
        {
            if (stopwatch == null) return;

            if (stopwatch.IsRunning)
            {
                //save the upload time and file size to calculate average upload speed
                AddReading(stopwatch.Elapsed, fileSizeBytes);
            }

            stopwatch.Stop();
            timer.Stop();
        }

        public (double UploadProgress, string UploadTime) GetUploadProgress()
        {
            var uploadTimerText = stopwatch.Elapsed.ToString(@"mm\:ss");
            var uploadProgress = stopwatch.Elapsed.TotalSeconds / estimatedUploadTime.TotalSeconds;
            log.Trace($"Upload Progress: {uploadProgress}, Upload Time: {uploadTimerText}");

            return (uploadProgress, uploadTimerText);
        }
    }

    public interface IUploadTimerService
    {
        void AddReading(TimeSpan time, long fileSizeBytes);

        TimeSpan EstimateUploadTime(long fileSizeBytes);

        void StartUploadTimer(long fileSizeBytes, Action timerUpdateCallback);

        void StopUploadTimer();

        (double UploadProgress, string UploadTime) GetUploadProgress();
    }
}
