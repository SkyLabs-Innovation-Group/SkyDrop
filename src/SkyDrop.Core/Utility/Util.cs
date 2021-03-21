using System;
namespace SkyDrop.Core.Utility
{
    public static class Util
    {
        public const string Portal = "https://siasky.net";

        public static string GetSkylinkUrl(string skylink)
        {
            return $"{Portal}/{skylink}";
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
