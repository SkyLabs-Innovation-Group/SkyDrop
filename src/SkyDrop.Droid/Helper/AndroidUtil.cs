using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Util;
using Android.Widget;
using MvvmCross;
using Plugin.CurrentActivity;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;

namespace SkyDrop.Droid.Helper
{
    public static class AndroidUtil
    {
        public const int PickFileRequestCode = 100;
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
            if (uri.LastPathSegment.Contains("."))
            {
                //this is a "file"
                return uri.LastPathSegment;
            }

            //this is an "image"

            // The query, because it only applies to a single document, returns only
            // one row. There's no need to filter, sort, or select fields,
            // because we want all fields for one document.
            Android.Database.ICursor cursor = context.ContentResolver.Query(uri, null, null, null, null, null);
            var displayName = "";
            try
            {
                // moveToFirst() returns false if the cursor has 0 rows. Very handy for
                // "if there's anything to look at, look at it" conditionals.
                if (cursor != null && cursor.MoveToFirst())
                {
                    // Note it's called "Display Name". This is
                    // provider-specific, and might not necessarily be the file name.
                    displayName = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
                    Debug.WriteLine("Display Name: " + displayName);

                    var sizeIndex = cursor.GetColumnIndex(OpenableColumns.Size);
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
                    Debug.WriteLine("Size: " + size);
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
            Toast.MakeText(context, $"Opening file {file.Filename}", ToastLength.Long).Show();

            var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(Util.GetSkylinkUrl(file.Skylink)));
            context.StartActivity(browserIntent);
        }

        private static async Task<bool> CheckPermissions()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                log.Error("Permission not granted");
                log.Exception(ex);

                return false;
            }
        }

        public static Task<Bitmap> EncodeBarcode(BitMatrix matrix, int width, int height)
        {
            return Task.Run(() =>
            {
                try
                {
                    var darkGrey = Colors.DarkGrey.ToNative();
                    var lightGrey = Colors.LightGrey.ToNative();
                    var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            bitmap.SetPixel(x, y, GetBit(matrix, x, y) ? darkGrey : lightGrey);
                        }
                    }

                    return bitmap;
                }
                catch (WriterException ex)
                {
                    log.Exception(ex);
                    return null;
                }
            });
        }

        public static bool GetBit(BitMatrix matrix, int x, int y)
        {
            var row = matrix.getRow(y, null);
            return row[x];
        }
    }
}
