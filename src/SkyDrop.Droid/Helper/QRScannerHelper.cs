using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using ZXing.Mobile;

namespace SkyDrop.Droid.Helper
{
    /// <summary>
    ///     Enables us to show the scanner camera using the correct aspect ratio
    /// </summary>
    public static class QrScannerHelper
    {
        public static CameraResolution GetSquareScannerResolution(List<CameraResolution> availableResolutions)
        {
            var width = 1080;
            var height = 1080;
            return GetScannerResolution(availableResolutions, width, height);
        }

        //TODO: use this method to fix wrong aspect ratio on full screen scanner
        public static CameraResolution GetFullScreenScannerResolution(List<CameraResolution> availableResolutions)
        {
            var width = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait
                ? DeviceDisplay.MainDisplayInfo.Width
                : DeviceDisplay.MainDisplayInfo.Height;
            var height = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait
                ? DeviceDisplay.MainDisplayInfo.Height
                : DeviceDisplay.MainDisplayInfo.Width;
            return GetScannerResolution(availableResolutions, width, height);
        }

        private static CameraResolution GetScannerResolution(List<CameraResolution> availableResolutions, double width,
            double height)
        {
            CameraResolution result = null;
            //a tolerance of 0.1 should not be visible to the user
            var aspectTolerance = 0.1;
            //calculatiing our targetRatio
            var targetRatio = height / width;
            var targetHeight = height;
            var minDiff = double.MaxValue;
            //camera API lists all available resolutions from highest to lowest, perfect for us
            //making use of this sorting, following code runs some comparisons to select the lowest resolution that matches the screen aspect ratio and lies within tolerance
            //selecting the lowest makes Qr detection actual faster most of the time
            foreach (var r in availableResolutions.Where(r =>
                         Math.Abs((double)r.Width / r.Height - targetRatio) < aspectTolerance))
            {
                //slowly going down the list to the lowest matching solution with the correct aspect ratio
                if (Math.Abs(r.Height - targetHeight) < minDiff)
                    minDiff = Math.Abs(r.Height - targetHeight);
                result = r;
            }

            return result;
        }
    }
}