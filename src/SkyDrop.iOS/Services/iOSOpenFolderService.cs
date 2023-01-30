using System;
using Foundation;
using System.Security.Policy;
using SkyDrop.Core.Services;
using UIKit;

namespace SkyDrop.iOS.Services
{
    public class iOSOpenFolderService : IOpenFolderService
    {
        public void OpenFolder()
        {
            var path = GetDocumentsDirectory().AbsoluteString.Replace("file://", "shareddocuments://");
            var url = new NSUrl(path);

            UIApplication.SharedApplication.OpenUrl(url);
        }

        private NSUrl GetDocumentsDirectory()
        {
            var paths = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User);
            var documentsDirectory = paths[0];
            return documentsDirectory;
        }
    }
}

