using Android.Content;
using Android.Net;
using Plugin.CurrentActivity;
using SkyDrop.Core.Services;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Services
{
    public class AndroidOpenFolderService : IOpenFolderService
    {
        private readonly IFileSystemService fileSystemService;

        public AndroidOpenFolderService(IFileSystemService fileSystemService)
        {
            this.fileSystemService = fileSystemService;
        }

        public void OpenFolder()
        {
            var intent = new Intent(Intent.ActionGetContent);
            var uri = Uri.Parse(fileSystemService.DownloadsFolderPath);
            intent.SetDataAndType(uri, "*/*");

            if (CrossCurrentActivity.Current?.Activity == null)
                return;

            CrossCurrentActivity.Current.Activity.StartActivity(Intent.CreateChooser(intent, "Open folder"));
        }
    }
}

