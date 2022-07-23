using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.IO.Pem;
using SkyDrop.Core.DataModels;

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

        public static string PublicKeyToBase64String(this X25519PublicKeyParameters key)
        {
            return Convert.ToBase64String(key.GetEncoded());
        }

        public static X25519PublicKeyParameters Base64StringToPublicKey(this string key)
        {
            var bytes = Convert.FromBase64String(key);
            return new X25519PublicKeyParameters(bytes);
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        /// <summary>
        /// Enables matching extensions with multiple periods correctly e.g. .jpeg.skydrop
        /// </summary>
        public static string GetFilenameExtension(string filename)
        {
            return Regex.Match(filename, @"\..*").Value;
        }
    }
}
