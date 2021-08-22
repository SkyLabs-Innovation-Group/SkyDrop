using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using MvvmCross;
using Plugin.CurrentActivity;
using SkyDrop.Core.DataModels;
using SkyDrop.Droid.Views.Main;
using ZXing.Common;
using ZXing.Mobile;
using static SkyDrop.Core.ViewModels.Main.DropViewModel;

namespace SkyDrop.Droid.Helper
{
    public static class AndroidUtil
    {
        public const int PickFileRequestCode = 100;

        private const string UploadNotificationChannelId = "to.hns.skydrop.notifications.DOWNLOAD";
        private const string UploadNotificationChannelName = "Uploads";
        private const string UploadNotificationChannelDescription = "Notifies you when a file has finished uploading";
        private const int UploadNotificationId = 200;

        private static NotificationCompat.Builder uploadNotificationBuilder;

        private static readonly ILog log = Mvx.IoCProvider.Resolve<ILog>();

        public static int DpToPx(int dp)
        {
            return (int)Math.Round(CrossCurrentActivity.Current.AppContext.Resources.DisplayMetrics.Density * dp);
        }

        /// <summary>
        /// Get the filename for a local file on Android
        /// </summary>
        public static string GetFileName(Context context, Android.Net.Uri uri)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string displayName;
            Android.Database.ICursor cursor = null;
            try
            {
                if (uri.LastPathSegment.Contains("."))
                {
                    //this is a "file"
                    return uri.LastPathSegment;
                }

                //this is an "image"

                // The query, because it only applies to a single document, returns only
                // one row. There's no need to filter, sort, or select fields,
                // because we want all fields for one document.
                cursor = context.ContentResolver.Query(uri, null, null, null, null, null);
                
                // moveToFirst() returns false if the cursor has 0 rows. Very handy for
                // "if there's anything to look at, look at it" conditionals.
                if (cursor != null && cursor.MoveToFirst())
                {
                    // Note it's called "Display Name". This is
                    // provider-specific, and might not necessarily be the file name.
                    displayName = cursor.GetString(cursor.GetColumnIndex(Android.Provider.IOpenableColumns.DisplayName));
                    log.Trace("Display Name: " + displayName);

                    var sizeIndex = cursor.GetColumnIndex(Android.Provider.IOpenableColumns.Size);
                    // If the size is unknown, the value stored is null. But because an
                    // int can't be null, the behavior is implementation-specific,
                    // and unpredictable. So as
                    // a rule, check if it's null before assigning to an int. This will
                    // happen often: The storage API allows for remote files, whose
                    // size might not be locally known.
                    string size = null;
                    if (!cursor.IsNull(sizeIndex))
                    {
                        // Technically the column stores an int, but cursor.getString()
                        // will do the conversion automatically.
                        size = cursor.GetString(sizeIndex);
                    }
                    else
                    {
                        size = "Unknown";
                    }
                    log.Trace("Size: " + size);
                }
                else
                {
                    return "NONAME";
                }
            }
            catch (Exception e)
            {
                return "error.jpg";
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }

            return displayName;
        }

        public static void OpenFileInBrowser(Activity context, SkyFile file)
        {
            Toast.MakeText(context, $"Opening file {file.Filename}", ToastLength.Long)?.Show();

            var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(file.GetSkylinkUrl()));
            context.StartActivity(browserIntent);
        }

        public static bool GetBit(BitMatrix matrix, int x, int y)
        {
            var row = matrix.getRow(y, null);
            return row[x];
        }

        public static Task<Bitmap> BitMatrixToBitmap(BitMatrix bitMatrix)
        {
            var renderer = new BitmapRenderer();
            return Task.Run(() =>
            {
                //computationally heavy but quick
                return renderer.Render(bitMatrix, ZXing.BarcodeFormat.QR_CODE, "");
            });
        }

        /// <summary>
        /// Creates a "channel" (category) for local notifications, required from Android O
        /// Call this in OnCreate()
        /// </summary>
        public static void CreateNotificationChannel(Context context)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = UploadNotificationChannelName;
            var channelDescription = UploadNotificationChannelDescription;
            var channel = new NotificationChannel(UploadNotificationChannelId, new Java.Lang.String(channelName), NotificationImportance.Default)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public static void ShowUploadStartedNotification(Context context, string message)
        {
            // Set up an intent so that tapping the notifications returns to this app:
            Intent intent = new Intent(context, typeof(DropView));

            //prevent the activity from being restarted (retain state)
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(context, pendingIntentId, intent, 0);

            // Instantiate the builder and set notification elements:
            uploadNotificationBuilder = new NotificationCompat.Builder(context, UploadNotificationChannelId)
                .SetContentTitle("Sending file...")
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.ic_skydrop)
                .SetContentIntent(pendingIntent)
                .SetProgress(100, 0, false)
                .SetAutoCancel(true); //dismiss notification when tapped

            // Build the notification:
            Notification notification = uploadNotificationBuilder.Build();

            // Publish the notification:
            GetNotificationManager(context).Notify(UploadNotificationId, notification);
        }

        public static void ShowUploadFinishedNotification(Context context, FileUploadResult uploadResult)
        {
            switch(uploadResult)
            {
                case FileUploadResult.Success:
                    uploadNotificationBuilder.SetContentTitle("File published successfully (tap to view)");
                    uploadNotificationBuilder.SetProgress(0, 0, false); //hide progressbar
                    break;
                case FileUploadResult.Fail:
                    uploadNotificationBuilder.SetContentTitle("Upload failed");
                    uploadNotificationBuilder.SetProgress(0, 0, false); //hide progressbar
                    break;
                case FileUploadResult.Cancelled:
                    GetNotificationManager(context).CancelAll();
                    return;
            }

            // Build a notification object with updated content:
            var notification = uploadNotificationBuilder.Build();

            // Publish the new notification with the existing ID:
            GetNotificationManager(context).Notify(UploadNotificationId, notification);
        }

        public static void UpdateNotificationProgress(Context context, double normalProgress)
        {
            if (normalProgress >= 1)
            {
                //set indeterminate loader
                uploadNotificationBuilder.SetProgress(100, 0, true);
                var notification = uploadNotificationBuilder.Build();
                GetNotificationManager(context).Notify(UploadNotificationId, notification);
                return;
            }

            var intProgress = (int)Math.Floor(normalProgress * 100);
            uploadNotificationBuilder.SetProgress(100, intProgress, false);
            var notificationOther = uploadNotificationBuilder.Build();
            GetNotificationManager(context).Notify(UploadNotificationId, notificationOther);
        }

        private static NotificationManager GetNotificationManager(Context context)
        {
            return context.GetSystemService(Context.NotificationService) as NotificationManager;
        }
    }
}
