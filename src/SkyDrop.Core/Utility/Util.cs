using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross;
using Org.BouncyCastle.Crypto.Parameters;
using Xamarin.Essentials;

namespace SkyDrop.Core.Utility
{
    public static class Util
    {
        public enum FileCategory
        {
            None,
            Document,
            Image,
            Audio,
            Video,
            Zip,
            Encrypted
        }

        public enum SaveType
        {
            Cancel, //do nothing
            Photos, //save image to gallery
            Files //save to files
        }

        public const float NavDotsMinAlpha = 0.2f;
        public const float NavDotsMaxAlpha = 0.8f;

        public static string GetFileSizeString(long bytesCount)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double bytes = bytesCount;
            var order = 0;
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
            if (filename.IsNullOrEmpty())
                return FileCategory.None;

            filename = filename.ToLowerInvariant();

            if (filename.ExtensionMatches(".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".ods", ".odt", ".pdf",
                    ".txt", ".html", ".htm"))
                return FileCategory.Document;

            if (filename.ExtensionMatches(".jpg", ".jpeg", ".bmp", ".png", ".gif", ".webp", ".tiff", ".psd", ".raw",
                    ".svg", ".heic"))
                return FileCategory.Image;

            if (filename.ExtensionMatches(".wav", ".mp3", ".aac", ".ogg", ".aiff", ".wma", ".flac", ".alac"))
                return FileCategory.Audio;

            if (filename.ExtensionMatches(".mp4", ".m4p", ".m4v", ".mov", ".avi", ".webm", ".mpg", ".mp2", ".mpeg",
                    ".mpe", ".mpv", ".wmv", ".qt", ".mkv"))
                return FileCategory.Video;

            if (filename.ExtensionMatches(".zip"))
                return FileCategory.Zip;

            if (filename.IsEncryptedFile())
                return FileCategory.Encrypted;

            return FileCategory.None;
        }

        public static bool CanDisplayPreview(this string filename)
        {
            if (filename == null)
                return false;

            filename = filename.ToLowerInvariant();
            return filename.ExtensionMatches(".jpg", ".jpeg", ".bmp", ".png", ".gif", ".webp", ".heic");
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        public static string PublicKeyToBase64String(this X25519PublicKeyParameters key)
        {
            return Convert.ToBase64String(key.GetEncoded());
        }

        public static X25519PublicKeyParameters Base64StringToPublicKey(this string key)
        {
            var bytes = Convert.FromBase64String(key);
            return new X25519PublicKeyParameters(bytes);
        }

        /// <summary>
        /// Concatenate byte arrays
        /// </summary>
        public static byte[] Combine(params byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }

        /// <summary>
        /// Enables matching extensions with multiple periods correctly e.g. .jpeg.pdf
        /// </summary>
        public static string GetFullExtension(string filename)
        {
            var lastSlashIndex = filename.LastIndexOf("/") + 1;
            filename = filename.Substring(lastSlashIndex, filename.Length - lastSlashIndex);
            return Regex.Match(filename, @"\..*").Value;
        }

        public static async Task<SaveType> GetSaveType(string filename)
        {
            if (!filename.CanDisplayPreview())
                //not an image
                return SaveType.Files;

            //file is an image

            if (DeviceInfo.Platform == DevicePlatform.Android)
                //always save images to gallery on Android
                return SaveType.Photos;

            //show gallery / files menu on iOS

            return await SaveTypePromptAsync(false);
        }

        public static async Task<SaveType> GetSaveTypeForMultiple(List<string> filenames)
        {
            if (!filenames.Any(f => f.CanDisplayPreview()))
                //none of the files are images
                return SaveType.Files;

            if (DeviceInfo.Platform == DevicePlatform.Android)
                //always save images to gallery on Android
                return SaveType.Photos;

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
            var result = await userDialogs.ActionSheetAsync($"Save image{s} to Photos or Files?", cancel, null, null,
                photos, files);
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

        public static bool IsEncryptedFile(this string filename)
        {
            if (filename.Length != 32) //length of a guid without dashes
                return false;

            var skSignatureExists = filename[15] == 's' && filename[16] == 'k'; //is encrypted file
            return skSignatureExists || filename.IsEncryptedZipFile();
        }

        public static bool IsEncryptedZipFile(this string filename)
        {
            if (filename?.Length != 32) //length of a guid without dashes
                return false;

            var ziSignatureExists = filename[15] == 'z' && filename[16] == 'i'; //is encrypted zip file
            return ziSignatureExists;
        }

        public static byte[] StreamToBytes(this Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0) ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }
    }
}