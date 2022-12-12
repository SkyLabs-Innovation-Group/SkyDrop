using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using MvvmCross;
using Plugin.CurrentActivity;
using SkyDrop.Core.Services;
using File = Java.IO.File;
using FileNotFoundException = Java.IO.FileNotFoundException;
using Uri = Android.Net.Uri;

namespace SkyDrop.Droid.Services
{
    public class AndroidSaveToGalleryService : ISaveToGalleryService
    {
        public async Task<string> SaveToGallery(Stream imageData, string filename)
        {
            var fileSystemService = Mvx.IoCProvider.Resolve<IFileSystemService>();
            var newPath = await fileSystemService.SaveFile(imageData, filename, true);
            NotifyGalleryOfNewMedia(newPath);

            return newPath;
        }

        private void NotifyGalleryOfNewMedia(string path)
        {
            try
            {
                var context = CrossCurrentActivity.Current.Activity;
                MediaStore.Images.Media.InsertImage(context.ContentResolver, path, Path.GetFileName(path), null);
                context.SendBroadcast(new Intent(Intent.ActionMediaScannerScanFile, Uri.FromFile(new File(path))));
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}