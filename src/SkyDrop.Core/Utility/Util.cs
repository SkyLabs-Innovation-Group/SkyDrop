using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.Core.Utility
{
    public static class Util
    {
        public const float NavDotsMinAlpha = 0.2f;
        public const float NavDotsMaxAlpha = 0.8f;
        
        public static string GetFileSizeString(long bytesCount)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double bytes = bytesCount;
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }

            //adjust the format string to your preferences. For example "{0:0.#}{1}" would
            //show a single decimal place, and no space.
            return $"{bytes:0.##}{sizes[order]}";
        }

        public static bool ExtensionMatches(this string filename, params string[] extensionsToMatch)
        {
            if (string.IsNullOrEmpty(filename))
                return false;

            foreach (var extension in extensionsToMatch)
                if (Path.GetExtension(filename).ToLower() == extension.ToLower())
                    return true;

            return false;
        }

        public static FileCategory GetFileCategory(string filename)
        {
            if (filename == null)
                return FileCategory.None;

            filename = filename.ToLowerInvariant();

            if (filename.ExtensionMatches(new[] { ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".ods", ".odt", ".pdf", ".txt", ".html", ".htm" }))
                return FileCategory.Document;

            if (filename.ExtensionMatches(new[] { ".jpg", ".jpeg", ".bmp", ".png", ".gif", ".webp", ".tiff", ".psd", ".raw", ".svg", ".heic" }))
                return FileCategory.Image;

            if (filename.ExtensionMatches(new[] { ".wav", ".mp3", ".aac", ".ogg", ".aiff", ".wma", ".flac", ".alac" }))
                return FileCategory.Audio;

            if (filename.ExtensionMatches(new[] { ".mp4", ".m4p", ".m4v", ".mov", ".avi", ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".wmv", ".qt", ".mkv" }))
                return FileCategory.Video;

            if (filename.ExtensionMatches(".zip"))
                return FileCategory.Zip;

            return FileCategory.None;
        }

        public enum FileCategory
        {
            None,
            Document,
            Image,
            Audio,
            Video,
            Zip
        }

        public static bool CanDisplayPreview(this string filename)
        {
            filename = filename.ToLowerInvariant();
            return filename.ExtensionMatches(".jpg", ".jpeg", ".bmp", ".png", ".gif", ".webp", ".heic");
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static async Task<SaveType> GetSaveType(string filename)
        {
            if (!Util.CanDisplayPreview(filename))
            {
                //not an image
                return SaveType.Files;
            }

            //file is an image

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                //always save images to gallery on Android
                return SaveType.Photos;
            }

            //show gallery / files menu on iOS

            return await SaveTypePromptAsync(false);
        }

        public static async Task<SaveType> GetSaveTypeForMultiple(List<string> filenames)
        {
            if (!filenames.Any(f => Util.CanDisplayPreview(f)))
            {
                //none of the files are images
                return SaveType.Files;
            }

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                //always save images to gallery on Android
                return SaveType.Photos;
            }

            //show gallery / files menu on iOS

            return await SaveTypePromptAsync(true);
        }

        private static async Task<SaveType> SaveTypePromptAsync(bool isPlural)
        {
            const string cancel = "Cancel";
            const string photos = "Photos";
            const string files = "Files";
            var userDialogs = Mvx.IoCProvider.Resolve<IUserDialogs>();
            var s = isPlural ? "s" : "";
            var result = await userDialogs.ActionSheetAsync($"Save image{s} to Photos or Files?", cancel, null, null, new[] { photos, files });
            switch (result)
            {
                case photos:
                    return SaveType.Photos;
                case files:
                    return SaveType.Files;
                case cancel:
                default:
                    return SaveType.Cancel;
            }
        }

        public enum SaveType
        {
            Cancel, //do nothing
            Photos, //save image to gallery
            Files //save to files
        }
    }
}
