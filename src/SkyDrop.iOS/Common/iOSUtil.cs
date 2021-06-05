using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using ZXing.Common;
using ZXing.Mobile;

namespace SkyDrop.iOS.Common
{
    public static class iOSUtil
    {
        public static Task<UIImage> BitMatrixToImage(BitMatrix bitMatrix)
        {
            var renderer = new BitmapRenderer();
            return Task.Run(() =>
            {
                //computationally heavy but quick
                return renderer.Render(bitMatrix, ZXing.BarcodeFormat.QR_CODE, "");
            });
        }

        public static void ShowUploadStartedNotification(string message)
        {
            /*
            // create the notification
            var notification = new UILocalNotification();

            // set the fire date (the date time in which it will fire)
            notification.FireDate = NSDate.FromTimeIntervalSinceNow(5);

            // configure the alert
            notification.AlertAction = "View Alert";
            notification.AlertBody = "Your one minute alert has fired!";

            // modify the badge
            notification.ApplicationIconBadgeNumber = 1;

            // set the sound to be the default sound
            notification.SoundName = UILocalNotification.DefaultSoundName;

            // schedule it
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            */
        }
    }
}
