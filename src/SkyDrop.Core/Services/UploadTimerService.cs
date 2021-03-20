using System;
using System.Collections.Generic;
using System.Linq;
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
            var estimatedSeconds = fileSizeBits * secondsPerBit;

            return TimeSpan.FromSeconds(estimatedSeconds);
        }
    }

    public interface IUploadTimerService
    {
        void AddReading(TimeSpan time, long fileSizeBytes);

        TimeSpan EstimateUploadTime(long fileSizeBytes);
    }
}
