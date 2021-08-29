using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Realms;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services.Api
{
    /// <summary>
    /// Keeps a running average of upload rates
    /// This is used to calculate estimated upload progress
    /// </summary>
    public class UploadTimerService : IUploadTimerService
    {
        private readonly ILog log;
        private readonly IStorageService storageService;

        public Stopwatch Stopwatch { get; set; }
        private Timer timer;

        public UploadTimerService(ILog log,
                                  IStorageService storageService)
        {
            this.log = log;
            this.storageService = storageService;
        }


        public void StartUploadTimer(long fileSizeBytes, Action timerUpdateCallback)
        {
            Stopwatch = new Stopwatch();
            timer = new Timer();
            timer.Elapsed += (s, e) => timerUpdateCallback();
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Start();
            Stopwatch.Start();
            timerUpdateCallback();
        }

        public void StopUploadTimer()
        {
            if (Stopwatch == null) return;

            Stopwatch.Stop();
            timer.Stop();
        }
    }

    public interface IUploadTimerService
    {
        Stopwatch Stopwatch { get; set; }

        void StartUploadTimer(long fileSizeBytes, Action timerUpdateCallback);

        void StopUploadTimer();
    }
}
