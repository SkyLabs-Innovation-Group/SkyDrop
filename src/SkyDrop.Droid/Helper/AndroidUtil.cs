using System;
using System.Diagnostics;
using System.IO;
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
using ZXing.Mobile;

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

        public static async Task SelectImage(Activity context)
        {
            if (!await CheckPermissions())
                return;
            /*
            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/*");
            context.StartActivityForResult(intent, AndroidUtil.PickFileRequestCode);
            */
            Intent getIntent = new Intent(Intent.ActionGetContent);
            getIntent.SetType("image/*");

            Intent pickIntent = new Intent(Intent.ActionPick, Android.Provider.MediaStore.Images.Media.ExternalContentUri);
            pickIntent.SetType("image/*");

            Intent chooserIntent = Intent.CreateChooser(getIntent, "Select Image");
            chooserIntent.PutExtra(Intent.ExtraInitialIntents, new Intent[] { pickIntent });

            context.StartActivityForResult(chooserIntent, AndroidUtil.PickFileRequestCode);
            //startActivityForResult(chooserIntent, PICK_IMAGE);
        }

        public static async Task SelectFile(Activity context)
        {
            try
            {
                if (!await CheckPermissions())
                    return;

                var intent = new Intent(Intent.ActionGetContent);
                intent.SetType("file/*");
                intent.AddCategory(Intent.CategoryOpenable);

                // special intent for Samsung file manager
                Intent sIntent = new Intent("com.sec.android.app.myfiles.PICK_DATA");
                sIntent.AddCategory(Intent.CategoryDefault);

                Intent chooserIntent;
                if (context.PackageManager.ResolveActivity(sIntent, 0) != null)
                {
                    // it is device with Samsung file manager
                    chooserIntent = Intent.CreateChooser(sIntent, "Open file");
                    chooserIntent.PutExtra(Intent.ExtraInitialIntents, new Intent[] { intent });
                }
                else
                {
                    chooserIntent = Intent.CreateChooser(intent, "Open file");
                }

                try
                {
                    context.StartActivityForResult(chooserIntent, PickFileRequestCode);
                }
                catch (Android.Content.ActivityNotFoundException ex)
                {
                    log.Exception(ex);
                    Toast.MakeText(context, "No suitable File Manager was found", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                log.Exception(ex);
            }
        }

        private static async Task<bool> CheckPermissions()
        {
            var permissionResult = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (permissionResult != PermissionStatus.Granted)
            {
                log.Error("StorageRead permission not granted.");
                return false;
            }

            return true;
        }

        public static async Task<SkyFile> HandlePickedFile(Activity context, Intent data)
        {
            //handle the selected file
            var uri = data.Data;
            string mimeType = context.ContentResolver.GetType(uri);
            log.Trace("mime type: " + mimeType);

            string extension = System.IO.Path.GetExtension(uri.Path);
            log.Trace("extension: " + extension);

            log.Trace("path: " + uri.Path);

            var filename = GetFileName(context, uri);

            //Toast.MakeText(context, uri.Path, ToastLength.Long).Show();

            var stream = GetFileStream(context, uri);
            return new SkyFile
            {
                FullFilePath = uri.Path,
                Filename = filename,
                FileSizeBytes = stream.Length,
            };

            
        }

        public static Stream GetFileStream(Activity context, Android.Net.Uri uri)
        {
            return context.ContentResolver.OpenInputStream(uri);
        }
    }
}
