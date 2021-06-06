using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using UserNotifications;
using ZXing.Common;
using ZXing.Mobile;
using static SkyDrop.Core.ViewModels.Main.DropViewModel;

namespace SkyDrop.iOS.Common
{
    public static class iOSUtil
    {
        private static UILocalNotification uploadNotification;

        public static Task<UIImage> BitMatrixToImage(BitMatrix bitMatrix)
        {
            var renderer = new BitmapRenderer();
            return Task.Run(() =>
            {
                //computationally heavy but quick
                return renderer.Render(bitMatrix, ZXing.BarcodeFormat.QR_CODE, "");
            });
        }

        public static void RegisterForLocalNotifications()
        {
            var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
        }

        public static void ShowUploadFinishedNotification(FileUploadResult uploadResult, string filename)
        {
            switch (uploadResult)
            {
                case FileUploadResult.Success:
                    ShowNotification("File published successfully (tap to view)", filename);
                    break;
                case FileUploadResult.Fail:
                    ShowNotification("Upload failed", filename);
                    break;
                case FileUploadResult.Cancelled:
                    break;
            }
        }

        public static void ShowNotification(string title, string message)
        {
            if (uploadNotification != null)
                UIApplication.SharedApplication.CancelLocalNotification(uploadNotification);

            uploadNotification = new UILocalNotification
            {
                FireDate = NSDate.FromTimeIntervalSinceNow(2),
                AlertTitle = title,
                AlertBody = message,
                ApplicationIconBadgeNumber = 0,
                SoundName = UILocalNotification.DefaultSoundName,
                UserInfo = new NSDictionary()
            };

            UIApplication.SharedApplication.ScheduleLocalNotification(uploadNotification);
        }
    }
}
