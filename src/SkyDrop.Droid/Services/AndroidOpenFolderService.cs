using Android.Content;
using Android.Net;
using Plugin.CurrentActivity;
using SkyDrop.Core.Services;
using SkyDrop.Droid.Helper;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Droid.Services
{
    public class AndroidOpenFolderService : IOpenFolderService
    {
        private readonly IFileSystemService fileSystemService;
        private readonly ILog log;

        public AndroidOpenFolderService(IFileSystemService fileSystemService, ILog log)
        {
            this.fileSystemService = fileSystemService;
            this.log = log;
        }

        public void OpenFolder(SaveType saveType)
        {
            try
            {
                var intent = new Intent(Intent.ActionGetContent);
                var uri = Uri.Parse(fileSystemService.DownloadsFolderPath);
                intent.SetDataAndType(uri, "*/*");

                CrossCurrentActivity.Current.Activity.StartActivity(Intent.CreateChooser(intent, "Open folder"));
            }
            catch(System.Exception e)
            {
                log.Exception(e);
            }
        }
    }
}

