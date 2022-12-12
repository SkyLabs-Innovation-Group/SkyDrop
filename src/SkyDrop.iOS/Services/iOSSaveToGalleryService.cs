using System.IO;
using System.Threading.Tasks;
using Foundation;
using SkyDrop.Core.Services;
using UIKit;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Services
{
    public class IOsSaveToGalleryService : ISaveToGalleryService
    {
        public async Task<string> SaveToGallery(Stream imageData, string filename)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var nsData = NSData.FromStream(imageData);
                var image = new UIImage(nsData);
                var taskCompletionSource = new TaskCompletionSource<bool>();
                image.SaveToPhotosAlbum((UIImage img, NSError error) => taskCompletionSource.SetResult(error == null));
                return taskCompletionSource.Task;
            });

            return filename;
        }
    }
}