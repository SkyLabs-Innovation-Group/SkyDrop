using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Provider;
using Java.IO;
using MvvmCross;
using SkyDrop.Core.Services;
using Xamarin.Essentials;

namespace SkyDrop.Droid.Services
{
    public class AndroidSaveToGalleryService : ISaveToGalleryService
    {
        public async Task<string> SaveToGallery(System.IO.Stream imageData, string filename)
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
                var context = Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity;
                MediaStore.Images.Media.InsertImage(context.ContentResolver, path, System.IO.Path.GetFileName(path), null);
                context.SendBroadcast(new Intent(Intent.ActionMediaScannerScanFile, Uri.FromFile(new File(path))));
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
	}
}

