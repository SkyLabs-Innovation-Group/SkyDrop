using System;
using System.Linq;

namespace SkyDrop.Core.Utility
{
    public static class Util
    {
        public const string Portal = "https://siasky.net";

        private const int SkylinkLength = 46;

        /// <summary>
        /// Convert raw skylink to full skylink url
        /// </summary>
        public static string GetSkylinkUrl(string skylink)
        {
            return $"{Portal}/{skylink}";
        }

        /// <summary>
        /// Convert full skylink url to raw skylink (removing any url parameters)
        /// </summary>
        public static string GetRawSkylink(string fullSkylinkUrl)
        {
            if (fullSkylinkUrl.Split('/').Last().Length != SkylinkLength)
                return null; //not a skylink

            return fullSkylinkUrl.Substring(fullSkylinkUrl.LastIndexOf('/') + 1, SkylinkLength);
        }

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
            return String.Format("{0:0.##}{1}", bytes, sizes[order]);
        }
    }
}
