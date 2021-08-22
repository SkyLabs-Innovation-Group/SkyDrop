using System;
using System.Linq;
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
    }
}
